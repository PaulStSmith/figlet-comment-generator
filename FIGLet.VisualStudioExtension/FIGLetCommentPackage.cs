using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace FIGLet.VisualStudioExtension;

/*
 *  ___ ___ ___ _        _    ___                         _   ___         _                 
 * | __|_ _/ __| |   ___| |_ / __|___ _ __  _ __  ___ _ _| |_| _ \__ _ __| |____ _ __ _ ___ 
 * | _| | | (_ | |__/ -_)  _| (__/ _ \ '  \| '  \/ -_) ' \  _|  _/ _` / _| / / _` / _` / -_)
 * |_| |___\___|____\___|\__|\___\___/_|_|_|_|_|_\___|_||_\__|_| \__,_\__|_\_\__,_\__, \___|
 *                                                                                |___/     
 */
/// <summary>
/// This class implements the package exposed by this assembly.
/// </summary>
/// <remarks>
/// The minimum requirement for a class to be considered a valid package for Visual Studio
/// is to implement the IVsPackage interface and register itself with the shell.
/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
/// to do it: it derives from the Package class that provides the implementation of the 
/// IVsPackage interface and uses the registration attributes defined in the framework to 
/// register itself and its components with the shell.
/// </remarks>
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuids.guidFIGLetCommentPackageString)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideOptionPage(typeof(FIGLetOptions), "FIGLet Comment Generator", "General", 0, 0, true)]
public sealed class FIGLetCommentPackage : AsyncPackage
{
    /*
     * Even though the FIGLetCommentCommand class is not used in this snippet, 
     * it is still necessary to include it in the package.
     * Without it -- and its initialization -- even though the menu item is added to the
     * Edit menu, the action will not be executed. 
     */

    /// <summary>
    /// Thread-safe singleton implementation of the FIGLetCommentCommand.
    /// </summary>
    private FIGLetCommentCommand command;

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that relies on services provided by VisualStudio.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>A task representing the async work of package initialization.</returns>
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        if (await GetServiceAsync(typeof(IMenuCommandService)) is IMenuCommandService commandService)
        {
            // Initialize the main Edit menu command
            command = new FIGLetCommentCommand(this, commandService);

            // Initialize context menu commands
            await command.InitializeContextMenuCommandsAsync();
        }
    }
}