using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FIGLet.VisualStudioExtension;

/// <summary>
/// Detects code elements within the active document in Visual Studio.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CodeElementDetector"/> class.
/// </remarks>
/// <param name="package">The async package.</param>
internal partial class CodeElementDetector(AsyncPackage package)
{
    /// <summary>
    /// The DTE2 object for interacting with the Visual Studio environment.
    /// </summary>
    private readonly DTE2 dte = package.GetService<DTE, DTE2>();

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

    /// <summary>
    /// Gets the FileCodeModel of the active document.
    /// </summary>
    /// <returns>The <see cref="FileCodeModel"/> of the active document or null if not found.</returns>
    private FileCodeModel FileCodeModel => ActiveDocument?.ProjectItem?.FileCodeModel;

    /// <summary>
    /// Gets the active document.
    /// </summary>
    /// <returns>The active <see cref="Document"/> or null if not found.</returns>
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
    public async Task<CodeElementInfo> GetCodeElementAtCursorAsync(Type enumeration = null)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        enumeration = enumeration ?? typeof(vsCMElement);
        if (enumeration.IsEnum == false)
            throw new ArgumentException("Enumeration type expected.", nameof(enumeration));

        try
        {
            if (FileCodeModel != null)
            {
                var selection = ActiveDocument.Selection as TextSelection;
                var ep = selection.ActivePoint.CreateEditPoint();
                CodeElement element;
                var values = Enum.GetValues(enumeration);
                foreach (var scope in values.Cast<vsCMElement>())
                {
                    try
                    {
                        element = FileCodeModel.CodeElementFromPoint(ep, scope) as CodeElement;
                    }
                    catch (COMException ex)
                    {
                        if (ex.ErrorCode == -2147467259) // E_FAIL
                            continue;   
                        throw;
                    }
                    if (element != null)
                    {
                        var fqName = element.FullName;
                        var (cn, mn) = ExtractClassAndMethodName(fqName, element);
                        return new CodeElementInfo(cn, mn, element.FullName, element);
                    }
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

    /// <summary>
    /// Gets the class and method name at the cursor position from the active code window.
    /// </summary>
    /// <returns>A tuple containing the class name and method name at the cursor position.</returns>
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

    /// <summary>
    /// Extracts the class and method name from the fully qualified name.
    /// </summary>
    /// <param name="fqName">The fully qualified name.</param>
    /// <param name="el">The code element (optional).</param>
    /// <returns>A tuple containing the class name and method name.</returns>
    private static (string className, string methodName) ExtractClassAndMethodName(string fqName, CodeElement el = null)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        fqName = fqName ?? string.Empty;
        if (fqName.Contains("("))
            fqName = fqName.Substring(0, fqName.IndexOf("("));

        var parts = fqName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

        if (el != null)
        {
            if (Enum.GetValues(typeof(VSClassLikeElement)).Cast<vsCMElement>().Contains((vsCMElement)el.Kind))
                return (parts[parts.Length - 1], null);
        }

        return parts.Length switch
        {
            0 => (null, null),
            1 => (parts[0], null),
            2 => (parts[0], parts[1]),
            _ => (parts[parts.Length - 2], parts[parts.Length - 1]),
        };
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
