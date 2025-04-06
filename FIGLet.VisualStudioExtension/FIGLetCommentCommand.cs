using EnvDTE;
using EnvDTE80;
using ByteForge.FIGLet.VisualStudioExtension.UI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ByteForge.FIGLet.VisualStudioExtension;

/*
 *  ___ ___ ___ _        _    ___                         _    ___                              _ 
 * | __|_ _/ __| |   ___| |_ / __|___ _ __  _ __  ___ _ _| |_ / __|___ _ __  _ __  __ _ _ _  __| |
 * | _| | | (_ | |__/ -_)  _| (__/ _ \ '  \| '  \/ -_) ' \  _| (__/ _ \ '  \| '  \/ _` | ' \/ _` |
 * |_| |___\___|____\___|\__|\___\___/_|_|_|_|_|_\___|_||_\__|\___\___/_|_|_|_|_|_\__,_|_||_\__,_|
 *                                                                                                
 */
/// <summary>
/// Command handler for FIGLet comments.
/// </summary>
internal sealed class FIGLetCommentCommand
{
    private readonly AsyncPackage package;
    private readonly IMenuCommandService commandService;
    private readonly CodeElementDetector detector;
    private readonly DTE2 dte;
    private readonly IInfoBarService infoBarService;

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
        this.infoBarService = new InfoBarService(package);

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
        var menuCommand = new OleMenuCommand(handler, commandID)
        {
            Text = text
        };
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

        if (sender is not OleMenuCommand command) return;

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

        var doc = GetActiveDocument();
        if (doc == null) return;

        // Show input dialog
        var dialogContent = new FIGLetInputDialogView(package, doc.Language, doc.FullName);
        if (DialogHelper.ShowDialog(dialogContent) != true)
            return;

        InsertBanner(dialogContent.PreviewBlock.Text);
    }

    private Document GetActiveDocument()
    {
        var doc = dte?.ActiveDocument;
        if (doc == null)
            infoBarService.ShowMessage("No active document found.");
        return doc;
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
            var doc = GetActiveDocument();
            if (doc == null) return;

            var _ = package.JoinableTaskFactory.RunAsync(async () =>
            {
                var ce = await detector.GetCodeElementAtCursorAsync(typeof(CodeElementDetector.VSClassLikeElement));

                // Switch back to UI thread for VS operations
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (string.IsNullOrEmpty(ce.ClassName))
                    return;

                // Show input dialog with class name pre-filled
                var dialogContent = new FIGLetInputDialogView(package, doc.Language, doc.FullName)
                {
                    InputText = ce.ClassName
                };

                /*
                 * Check if the element is a class, struct, interface, or enum.
                 */
                if (ce.CodeElement != null)
                {
                    if ((ce.CodeElement.Kind != vsCMElement.vsCMElementClass) && 
                        (ce.CodeElement.Kind != vsCMElement.vsCMElementStruct) &&
                        (ce.CodeElement.Kind != vsCMElement.vsCMElementInterface) &&
                        (ce.CodeElement.Kind != vsCMElement.vsCMElementEnum))
                    {
                        Debug.WriteLine("Element is not a class or struct.");
                        while ((ce.CodeElement.Kind != vsCMElement.vsCMElementClass) &&
                               (ce.CodeElement.Kind != vsCMElement.vsCMElementStruct) &&
                               (ce.CodeElement.Kind != vsCMElement.vsCMElementInterface) &&
                               (ce.CodeElement.Kind != vsCMElement.vsCMElementEnum))
                            ce.CodeElement = ce.CodeElement.Collection.Parent as CodeElement;
                    }
                }

                InsertCodeBanner(doc, ce, dialogContent);
            });
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
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
            var doc = GetActiveDocument();
            if (doc == null) return;

            var _ = package.JoinableTaskFactory.RunAsync(async () =>
            {
                var ce = await detector.GetCodeElementAtCursorAsync(typeof(CodeElementDetector.VSMemberElement));

                // Switch back to UI thread for VS operations
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (string.IsNullOrEmpty(ce.ClassName))
                    return;

                // Show input dialog with member name pre-filled
                var dialogContent = new FIGLetInputDialogView(package, doc.Language, doc.FullName)
                {
                    InputText = ce.MethodName
                };

                InsertCodeBanner(doc, ce, dialogContent);
            });
        }
        catch (Exception ex)
        {
            HandleException(ex);
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
            insertPoint = FindInsertionPoint(ce.CodeElement.StartPoint, doc.Language);
        }
        else
        {
            // Fallback to current selection (if any)
            var selection = (TextSelection)doc.Selection;
            insertPoint = FindInsertionPoint(selection.ActivePoint, doc.Language);
        }
        InsertBanner(dialogContent.PreviewBlock.Text, insertPoint);
    }

    /// <summary>
    /// Finds the insertion point for the banner starting from the given text point.
    /// </summary>
    /// <param name="startPoint">The starting text point.</param>
    /// <returns>The edit point where the banner should be inserted.</returns>
    /// <param name="language"></param>
    private EditPoint FindInsertionPoint(TextPoint startPoint, string language)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var insertPoint = startPoint.CreateEditPoint();

        // Get the line above the start point
        if (insertPoint.Line == 1)
            return insertPoint;
        var line = insertPoint.GetLines(insertPoint.Line - 1, insertPoint.Line).TrimStart();

        /*
         * We'll always insert the banner above the code element (or the current line).
         * However, we will not mess up with existing block comments or code decorations.
         */

        /*
         * Avoids a documentation/code decoration line.
         */
        while (IsDocOrDecorationLine(line, language) && insertPoint.Line > 1)
        {
            insertPoint.LineUp();
            line = insertPoint.GetLines(insertPoint.Line - 1, insertPoint.Line).TrimStart();
        }

        /*
         * Avoids a block comment.
         */
        var commentInfo = LanguageCommentStyles.GetCommentStyle(language);
        if (commentInfo.SupportsBlockComments && line.Contains(commentInfo.BlockCommentEnd))
        {
            // Move up until we find the start of the block comment
            while (!line.Contains(commentInfo.BlockCommentStart) && insertPoint.Line > 1)
            {
                insertPoint.LineUp();
                line = insertPoint.GetLines(insertPoint.Line - 1, insertPoint.Line).TrimStart();
            }
        }

        return insertPoint;
    }

    /// <summary>
    /// Determines if the given line is a documentation line or a code decoration based on the language.
    /// </summary>
    /// <param name="line">The line of text to check.</param>
    /// <param name="language">The programming language of the document.</param>
    /// <returns>True if the line is a documentation line or decoration, otherwise false.</returns>
    private bool IsDocOrDecorationLine(string line, string language)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        line = line.TrimStart();

        // Normalize language identifier
        language = language.ToLowerInvariant().Trim();

        // Group languages by their documentation style
        return language switch
        {
            // Triple-slash documentation style
            "csharp" or "fsharp" or "rust" => line.StartsWith("///") ||           // Doc comments
                                   line.StartsWith("["),// Attributes/Decorators
                                                        // Single-slash documentation
            "c/c++" or "cpp" or "d" or "objective-c" => line.StartsWith("///") ||           // Doc comments
                                   line.StartsWith("//!") ||           // Alternative doc comments
                                   line.StartsWith("@"),// Attributes (Objective-C)
                                                        // Triple-quote documentation
            "basic" or "vb" => line.StartsWith("'''") ||           // Doc comments
                                   line.StartsWith("<"),// Attributes
                                                        // Hash-based documentation
            "python" or "ruby" or "perl" or "yaml" or "shell" or "ps1" or "powershell" or "sh" or "zsh" or "bash" or "fish" or "shellscript" => line.StartsWith("#") ||             // Doc comments
                                   line.StartsWith("@"),// Decorators (Python)
                                                        // R-specific documentation
            "r" => line.StartsWith("#'"),// Roxygen2 doc comments
                                         // Fortran documentation
            "fortran" => line.StartsWith("!>") ||            // Doc comments
                                   line.StartsWith("!<"),// Alternative doc comments
                                                         // Lisp-family documentation
            "lisp" or "scheme" => line.StartsWith(";;;") ||           // Doc comments
                                   line.StartsWith(";;"),// Secondary doc comments
                                                         // SQL-family documentation
            "sql" or "tsql" or "mysql" or "pgsql" or "plsql" or "sqlite" => line.StartsWith("--") ||            // Doc comments
                                   line.StartsWith("--/"),// Alternative doc style (some dialects)
                                                          // Pascal documentation
            "pascal" => line.StartsWith("///") ||           // Doc comments
                                   line.StartsWith("//"),// Alternative doc comments
                                                         // Batch/DOS documentation
            "bat" or "cmd" or "dos" or "batch" => line.StartsWith("::") ||            // Doc comments
                                   line.StartsWith("rem", StringComparison.OrdinalIgnoreCase),// REM comments
                                                                                              // XML-style documentation
            "html" or "xml" or "xaml" or "svg" or "aspx" => line.StartsWith("<!--"),// XML comments
                                                                                    // Languages that don't typically use line-based documentation
            "java" or "javascript" or "typescript" or "css" or "go" or "swift" or "php" or "kotlin" or "scala" => line.StartsWith("@"),// Only check for annotations/decorators
                                                                                                                                       // (Their doc comments are typically block-based)
            _ => false,
        };
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

        var indentation = GenerateIndentation(insertPoint);
        var lines = bannerText.Split([ '\r', '\n' ], StringSplitOptions.RemoveEmptyEntries);

        insertPoint.StartOfLine();
        insertPoint.Insert(indentation + string.Join(Environment.NewLine + indentation, lines) + Environment.NewLine);
    }

    private string GenerateIndentation(EditPoint insertPoint)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var lineText = insertPoint.GetText(-insertPoint.LineCharOffset + 1);
        var m = Regex.Match(lineText, @"^[\t ]*");
        return m.Value;
    }
    
    static void HandleException(Exception ex, [CallerMemberName] string source = "")
    {
        var msg = $"Error executing {source}:{Environment.NewLine}{ex.Message}";
        Debug.WriteLine(msg);
        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}