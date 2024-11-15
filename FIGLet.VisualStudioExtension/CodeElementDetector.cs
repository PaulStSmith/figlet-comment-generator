using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FIGLet.VisualStudioExtension;

public readonly struct CodeElementInfo
{
    public string ClassName { get; }

    public string MethodName { get; }

    public string FullName { get; }

    public CodeElement CodeElement { get; }

    internal CodeElementInfo(string className, string methodName, string fullName, CodeElement codeElement)
    {
        ClassName = className;
        MethodName = methodName;
        FullName = fullName;
        CodeElement = codeElement;
    }

    public static CodeElementInfo Empty => new(null, null, null, null);

    public override string ToString()
    {
        return $"{ClassName}.{MethodName}";
    }
}

/// <summary>
/// Detects code elements within the active document in Visual Studio.
/// </summary>
internal class CodeElementDetector
{
    private readonly AsyncPackage package;
    private readonly DTE2 dte;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeElementDetector"/> class.
    /// </summary>
    /// <param name="package">The async package.</param>
    public CodeElementDetector(AsyncPackage package)
    {
        this.package = package;
        dte = package.GetService<DTE, DTE2>();
    }

    /// <summary>
    /// Gets the active code window.
    /// </summary>
    /// <returns>The active <see cref="IVsCodeWindow"/> or null if not found.</returns>
    private IVsCodeWindow ActiveCodeWindow
    {
        get
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the IVsMonitorSelection service
            var monitorSelection = (IVsMonitorSelection)ServiceProvider.GlobalProvider.GetService(typeof(SVsShellMonitorSelection));
            if (monitorSelection == null)
                return null;

            // Get the current active IVsWindowFrame
            if (monitorSelection.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out object activeFrame) != VSConstants.S_OK)
                return null;

            if (activeFrame is not IVsWindowFrame windowFrame)
                return null;

            // Get the IVsCodeWindow from the IVsWindowFrame
            if (windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var docView) != VSConstants.S_OK)
                return null;

            return docView as IVsCodeWindow;
        }
    }

    private FileCodeModel FileCodeModel
    {
        get
        {
            return ActiveDocument?.ProjectItem?.FileCodeModel;
        }
    }

    private Document ActiveDocument
    {
        get
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return dte.ActiveDocument;
        }
    }

    /// <summary>
    /// Gets the code element at the cursor asynchronously.
    /// </summary>
    /// <returns>A tuple containing the class name and method name at the cursor position.</returns>
    public async Task<CodeElementInfo> GetCodeElementAtCursorAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        try
        {
            if (FileCodeModel != null)
            {
                var selection = ActiveDocument.Selection as TextSelection;
                var element = FileCodeModel.CodeElementFromPoint(selection.ActivePoint, vsCMElement.vsCMElementFunction);

                if (element != null)
                {
                    var (cn, mn) = ExtractClassAndMethodName(element.FullName);
                    return new CodeElementInfo(cn, mn, element.FullName, element);
                }
            }

            var (cn2, mn2) = GetElementAtCursorFromWindow();
            return new CodeElementInfo(cn2, mn2, null, null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting code element: {ex.Message}");
        }

        return CodeElementInfo.Empty;
    }

    private (string className, string methodName) GetElementAtCursorFromWindow()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (ActiveCodeWindow is not IVsDropdownBarManager manager)
            return (null, null);

        var hr = manager.GetDropdownBar(out var bar);
        if (hr != VSConstants.S_OK || bar == null)
            return (null, null);

        var part1 = GetSelectionText(bar, 0);
        var part2 = GetSelectionText(bar, 1);
        var part3 = GetSelectionText(bar, 2);

        var fqName = $"{part1}.{part2}.{part3}";
        return ExtractClassAndMethodName(fqName);
    }

    private static (string className, string methodName) ExtractClassAndMethodName(string fqName)
    {
        fqName = fqName ?? string.Empty;
        if (fqName.Contains("("))
            fqName = fqName.Substring(0, fqName.IndexOf("("));

        var parts = fqName.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
        switch (parts.Length)
        {
            case 0: return (null, null);
            case 1: return (parts[0], null);
            case 2: return (parts[0], parts[1]);
            default: return (parts[parts.Length - 2], parts[parts.Length - 1]);
        }
    }

    /// <summary>
    /// Gets the selection text from the dropdown bar.
    /// </summary>
    /// <param name="bar">The dropdown bar.</param>
    /// <param name="barType">The type of the bar.</param>
    /// <returns>The selected text or null if not found.</returns>
    private string GetSelectionText(IVsDropdownBar bar, int barType)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (bar.GetCurrentSelection(barType, out var curSelection) != VSConstants.S_OK)
            return null;
        if (bar.GetClient(out var barClient) != VSConstants.S_OK)
            return null;
        if (barClient.GetEntryText(barType, curSelection, out var text) != VSConstants.S_OK)
            return null;
        return text;
    }
}
