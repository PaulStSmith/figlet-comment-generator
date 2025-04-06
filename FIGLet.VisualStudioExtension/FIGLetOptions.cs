using ByteForge.FIGLet.VisualStudioExtension.UI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace ByteForge.FIGLet.VisualStudioExtension;

/*
 *  ___ ___ ___ _        _    ___       _   _             
 * | __|_ _/ __| |   ___| |_ / _ \ _ __| |_(_)___ _ _  ___
 * | _| | | (_ | |__/ -_)  _| (_) | '_ \  _| / _ \ ' \(_-<
 * |_| |___\___|____\___|\__|\___/| .__/\__|_\___/_||_/__/
 *                                |_|                     
 */
/// <summary>
/// Represents the options page for the FIGLet Visual Studio extension.
/// </summary>
[Guid(PackageGuids.guidFIGLetCommentPackageOptionsString)]  // Generate new GUID
public class FIGLetOptions : UIElementDialogPage, IServiceProvider
{
    private FIGLetOptionsControl _page;

    /// <summary>
    /// Gets or sets the default font for FIGLet comments.
    /// </summary>
    public string LastSelectedFont { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the layout mode for FIGLet comments.
    /// </summary>
    public LayoutMode LayoutMode { get; set; } = LayoutMode.Smushing;

    /// <summary>
    /// Gets or sets the custom font directory for FIGLet comments.
    /// </summary>
    public string FontPath { get; set; } = "";

    /// <summary>
    /// Gets the UI element to be hosted in the options page.
    /// </summary>
    protected override UIElement Child => _page ??= new FIGLetOptionsControl(this);

    /// <summary>
    /// Loads the settings from storage.
    /// </summary>
    public override void LoadSettingsFromStorage()
    {
        base.LoadSettingsFromStorage();
        _page?.UpdateControls();
    }

    /// <summary>
    /// Saves the settings to storage.
    /// </summary>
    public override void SaveSettingsToStorage()
    {
        _page?.UpdateSettings();
        base.SaveSettingsToStorage();
    }


    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>A service object of type <paramref name="serviceType"/>.</returns>
    /// <remarks>
    /// Even though <see cref="UIElementDialogPage"/> has a <see cref="GetService"/> method, the class
    /// hierarchy, at no point, implements the <see cref="IServiceProvider"/> interface. 
    /// At some point, I needed this class to implement the interface, so here it is.
    /// </remarks>
    object IServiceProvider.GetService(Type serviceType)
    {
        return this.GetService(serviceType);
    }
}