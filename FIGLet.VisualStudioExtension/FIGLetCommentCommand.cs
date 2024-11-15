using EnvDTE;
using EnvDTE80;
using FIGLet.VisualStudioExtension.UI;
using Microsoft.VisualStudio.RpcContracts.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FIGLet.VisualStudioExtension;

/// <summary>
/// Command handler for FIGLet comments.
/// </summary>
internal sealed class FIGLetCommentCommand
{
    private readonly AsyncPackage package;
    private readonly IMenuCommandService commandService;
    private readonly CodeElementDetector detector;
    private readonly DTE2 dte;

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetCommentCommand"/> class.
    /// Adds the command to the command service.
    /// </summary>
    /// <param name="package">The package.</param>
    /// <param name="commandService">The command service.</param>
    /// 
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
    public FIGLetCommentCommand(AsyncPackage package, IMenuCommandService commandService)
    {
        this.package = package ?? throw new ArgumentNullException(nameof(package));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.dte = package.GetService<SDTE, DTE2>();
        this.detector = new CodeElementDetector(package);

        var menuCommandID = new CommandID(PackageGuids.guidFIGLetCommentPackageCmdSet, PackageIds.FIGLetCommentCommandId);
        var oleMenuItem = new OleMenuCommand(Execute, menuCommandID);  // Changed to OleMenuCommand
        oleMenuItem.BeforeQueryStatus += UpdateCommandStatus;
        commandService.AddCommand(oleMenuItem);
    }

    /// <summary>
    /// Initializes context menu commands asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeContextMenuCommandsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        // Add context menu group
        var contextMenuGroup = new CommandID(PackageGuids.guidFIGLetContextMenuCmdSet, PackageIds.FIGLetContextMenuGroup);
        var contextMenuItem = new MenuCommand((s, e) => { }, contextMenuGroup);
        commandService.AddCommand(contextMenuItem);

        // Add context menu commands
        AddContextMenuCommand(PackageIds.InsertFIGLetBannerCommandId, "Insert FIGLet Banner", ExecuteGenericBanner);
        AddContextMenuCommand(PackageIds.InsertFIGLetClassBannerCommandId, "Insert FIGLet Class Banner", ExecuteClassBanner);
        AddContextMenuCommand(PackageIds.InsertFIGLetMethodBannerCommandId, "Insert FIGLet Method Banner", ExecuteMethodBanner);
    }

    /// <summary>
    /// Adds a context menu command.
    /// </summary>
    /// <param name="commandId">The command ID.</param>
    /// <param name="text">The text for the command.</param>
    /// <param name="handler">The event handler for the command.</param>
    private void AddContextMenuCommand(int commandId, string text, EventHandler handler)
    {
        var commandID = new CommandID(PackageGuids.guidFIGLetContextMenuCmdSet, commandId);
        var menuCommand = new OleMenuCommand(handler, commandID);
        menuCommand.BeforeQueryStatus += UpdateCommandStatus;
        commandService.AddCommand(menuCommand);
    }

    /// <summary>
    /// Updates the status of the command.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void UpdateCommandStatus(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var command = sender as OleMenuCommand;
        if (command == null) return;

        // Enable command only if we have an active text document
        var isEnabled = IsTextEditorActive();
        command.Enabled = isEnabled;
        command.Visible = isEnabled;
    }

    /// <summary>
    /// Checks if the active document is a text editor.
    /// </summary>
    /// <returns>True if the active document is a text editor, otherwise false.</returns>
    private bool IsTextEditorActive()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (dte?.ActiveDocument == null)
            return false;

        return dte.ActiveDocument.Object("TextDocument") is TextDocument;
    }

    /// <summary>
    /// Executes the command to insert a generic FIGLet banner.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Execute(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        ExecuteGenericBanner(sender, e);
    }

    /// <summary>
    /// Executes the command to insert a generic FIGLet banner.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ExecuteGenericBanner(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var doc = dte?.ActiveDocument;
        if (doc == null) return;

        // Show input dialog
        var dialogContent = new FIGLetInputDialogView(package, doc.Language);
        if (DialogHelper.ShowDialog(dialogContent) != true)
            return;

        InsertBanner(dialogContent.PreviewBlock.Text);
    }

    /// <summary>
    /// Executes the command to insert a FIGLet banner for a class.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ExecuteClassBanner(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            var doc = dte?.ActiveDocument;
            if (doc == null) return;

            var _ = package.JoinableTaskFactory.RunAsync(async () =>
            {
                var ce = await detector.GetCodeElementAtCursorAsync();

                // Switch back to UI thread for VS operations
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (string.IsNullOrEmpty(ce.ClassName))
                    return;

                // Show input dialog with class name pre-filled
                var dialogContent = new FIGLetInputDialogView(package, doc.Language)
                {
                    InputText = ce.ClassName
                };

                InsertCodeBanner(doc, ce, dialogContent);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error executing class banner: {ex.Message}");
        }
    }

    /// <summary>
    /// Inserts a FIGLet banner into the code at the specified location.
    /// </summary>
    /// <param name="doc">The active document.</param>
    /// <param name="ce">The code element information.</param>
    /// <param name="dialogContent">The dialog content containing the banner text.</param>
    private void InsertCodeBanner(Document doc, CodeElementInfo ce, FIGLetInputDialogView dialogContent)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (DialogHelper.ShowDialog(dialogContent) != true)
            return;

        EditPoint insertPoint;
        if (ce.CodeElement != null)
        {
            insertPoint = FindInsertionPoint(ce.CodeElement.StartPoint);
        }
        else
        {
            // Fallback to current selection (if any)
            var selection = (TextSelection)doc.Selection;
            insertPoint = FindInsertionPoint(selection.ActivePoint);
        }
        InsertBanner(dialogContent.PreviewBlock.Text, insertPoint);
    }

    /// <summary>
    /// Executes the command to insert a FIGLet banner for a method.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ExecuteMethodBanner(object sender, EventArgs e)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            var doc = dte?.ActiveDocument;
            if (doc == null) return;

            var _ = package.JoinableTaskFactory.RunAsync(async () =>
            {
                var ce = await detector.GetCodeElementAtCursorAsync();

                // Switch back to UI thread for VS operations
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (string.IsNullOrEmpty(ce.MethodName))
                    return;

                // Show input dialog with method name pre-filled
                var dialogContent = new FIGLetInputDialogView(package, doc.Language)
                {
                    InputText = ce.MethodName
                };

                InsertCodeBanner(doc, ce, dialogContent);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error executing method banner: {ex.Message}");
        }
    }

    /// <summary>
    /// Finds the insertion point for the banner starting from the given text point.
    /// </summary>
    /// <param name="startPoint">The starting text point.</param>
    /// <returns>The edit point where the banner should be inserted.</returns>
    private EditPoint FindInsertionPoint(TextPoint startPoint)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var insertPoint = startPoint.CreateEditPoint();
        var doc = dte.ActiveDocument;
        var language = doc.Language?.ToLower();

        // Move up until we find a non-documentation line
        while (IsDocumentationLine(insertPoint, language))
        {
            insertPoint.LineUp();
        }

        return insertPoint;
    }

    /// <summary>
    /// Determines if the given edit point is at a documentation line based on the language.
    /// </summary>
    /// <param name="point">The edit point to check.</param>
    /// <param name="language">The programming language of the document.</param>
    /// <returns>True if the line is a documentation line, otherwise false.</returns>
    private bool IsDocumentationLine(EditPoint point, string language)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var line = point.GetLines(point.Line, point.Line + 1).TrimStart();

        switch (language)
        {
            case "csharp":
                return line.StartsWith("///");
            case "c/c++":
                return line.StartsWith("/**") || line.StartsWith("*/") || line.StartsWith("*");
            case "python":
                return line.StartsWith("\"\"\"") || line.StartsWith("'''");
            // Add more languages as needed
            default:
                return false;
        }
    }

    /// <summary>
    /// Inserts a FIGLet banner at the specified insertion point or at the current selection if no insertion point is provided.
    /// </summary>
    /// <param name="bannerText">The text of the FIGLet banner to insert.</param>
    /// <param name="insertPoint">The edit point where the banner should be inserted. If null, the current selection is used.</param>
    private void InsertBanner(string bannerText, EditPoint insertPoint = null)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var doc = dte.ActiveDocument;
        if (doc == null) return;

        var selection = (TextSelection)doc.Selection;
        insertPoint ??= selection.ActivePoint.CreateEditPoint();

        var indentation = GenerateIndentation(doc);
        var lines = bannerText.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

        insertPoint.StartOfLine();
        insertPoint.Insert(indentation + string.Join(Environment.NewLine + indentation, lines) + Environment.NewLine);
    }

    /// <summary>
    /// Generates the indentation string for the current line in the document.
    /// </summary>
    /// <param name="doc">The active document.</param>
    /// <returns>The indentation string.</returns>
    private string GenerateIndentation(Document doc)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        // Get the current line's indentation
        var selection = (TextSelection)doc.Selection;
        var ep = selection.ActivePoint.CreateEditPoint();
        var lineText = ep.GetText(-ep.LineCharOffset + 1);
        var m = Regex.Match(lineText, @"^[\t ]*");
        if (m.Success) return m.Value;

        /*
         * Line indentation could not be determined from the text selection.
         * So, we'll try to determine the indentation based on the language settings
         * and the current cursor position.
         */
        var lineCharOffset = ep.LineCharOffset;

        // Determine the language of the active document
        var language = doc.Language;

        // Access the text editor properties for the active language
        Properties languageProperties;
        try
        {
            languageProperties = doc.DTE.Properties["TextEditor", language];
        }
        catch (COMException ex)
        {
            Debug.WriteLine($"Failed to retrieve language properties for {language}: {{{ex.GetType()}}} {ex.Message}");
            // Fallback to all languages if the specific language properties are not available
            languageProperties = doc.DTE.Properties["TextEditor", "AllLanguages"];
        }

        // Retrieve specific tab settings
        var tabSize = TryGetValue(languageProperties, "TabSize", 4);
        var useSpaces = TryGetValue(languageProperties, "InsertSpaces", true);

        if (useSpaces)
            return new string(' ', lineCharOffset);
        else
        {
            // Calculate the number of tabs and spaces needed
            var spacesToNextTabStop = tabSize - (lineCharOffset % tabSize);
            if (spacesToNextTabStop == tabSize)
                spacesToNextTabStop = 0;

            var tabsNeeded = spacesToNextTabStop / tabSize;
            var spacesNeeded = spacesToNextTabStop % tabSize;

            return new string('\t', tabsNeeded) + new string(' ', spacesNeeded);
        }
    }
    
    /// <summary>
    /// Tries to get the value of a property from the given properties collection.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="properties">The properties collection.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value to return if the property is not found.</param>
    /// <returns>The value of the property if found, otherwise the default value.</returns>
    T TryGetValue<T>(Properties properties, string name, T defaultValue)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            return (T)properties.Item(name).Value;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to retrieve property value: {{{ex.GetType()}}} {ex.Message}");
            return defaultValue;
        }
    }
}