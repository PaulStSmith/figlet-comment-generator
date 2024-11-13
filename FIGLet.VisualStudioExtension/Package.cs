using EnvDTE;
using FIGLet.VisualStudioExtension.UI;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace FIGLet.VisualStudioExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidFIGLetCommentPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(FIGLetOptions), "FIGLet Comment Generator", "General", 0, 0, true)]
    public sealed class FIGLetCommentPackage : AsyncPackage
    {
        // Thread-safe singleton implementation
        private FIGLetCommentCommand command;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (await GetServiceAsync(typeof(IMenuCommandService)) is IMenuCommandService commandService)
            {
                var renderer = new FIGLetRenderer();
                command = new FIGLetCommentCommand(this, commandService, renderer);
            }
        }
    }

    internal sealed class FIGLetCommentCommand
    {
        private readonly AsyncPackage package;
        private readonly FIGLetRenderer renderer;

        public FIGLetCommentCommand(AsyncPackage package, IMenuCommandService commandService, FIGLetRenderer renderer)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

            var menuCommandID = new CommandID(PackageGuids.guidFIGLetCommentPackageCmdSet, PackageIds.FIGLetCommentCommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        internal static class PackageIds
        {
            public const int MyMenuGroup = 0x1020;
            public const int FIGLetCommentCommandId = 0x0100;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the active document
            var dte = package.GetService<DTE, DTE>();
            if (dte?.ActiveDocument == null)
                return;

            var activeDocument = dte.ActiveDocument;
            var lang = activeDocument.Language;

            // Show our custom input dialog
            var dialogContent = new FIGLetInputDialogView(package, renderer, lang);

            if (DialogHelper.ShowDialog(dialogContent) != true)
                return;

            // Get current options
            var options = (FIGLetOptions)package.GetDialogPage(typeof(FIGLetOptions));

            // Generate FIGLet text using the input from dialog
            var FIGLetText = renderer.Render(dialogContent.InputText, options.LayoutMode);

            // Convert to comment based on file type
            var commentedText = LanguageCommentStyles.WrapInComments(FIGLetText, lang);

            // Insert at cursor position
            var selection = (EnvDTE.TextSelection)activeDocument.Selection;
            selection.Insert(commentedText + "\n");
        }
    }

    // PackageIds.cs
    internal static class PackageIds
    {
        public const int FIGLetCommentCommandId = 0x0100;
    }
}