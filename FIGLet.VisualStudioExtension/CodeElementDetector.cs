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
    public IVsCodeWindow GetActiveCodeWindow()
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

    /// <summary>
    /// Gets the code element at the cursor asynchronously.
    /// </summary>
    /// <returns>A tuple containing the class name and method name at the cursor position.</returns>
    public async Task<(string className, string methodName)> GetCodeElementAtCursorAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        try
        {
            if (GetActiveCodeWindow() is not IVsDropdownBarManager manager)
                return (null, null);

            var hr = manager.GetDropdownBar(out var bar);
            if (hr != VSConstants.S_OK || bar == null)
                return (null, null);

            var part1 = GetSelectionText(bar, 0);
            var part2 = GetSelectionText(bar, 1);
            var part3 = GetSelectionText(bar, 2);

            var fqName = $"{part1}.{part2}{(part3 == null ? "" : "." + part3)}".Split('.');

            var className = fqName[fqName.Length - 2];
            var methodName = fqName[fqName.Length - 1];

            return (className, methodName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting code element: {ex.Message}");
        }

        return (null, null);
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
