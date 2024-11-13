using FIGLet.VisualStudioExtension.UI;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace FIGLet.VisualStudioExtension
{
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
        /// Gets or sets the default insert location for FIGLet comments.
        /// </summary>
        public InsertLocation InsertLocation { get; set; } = InsertLocation.Above;

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

        object IServiceProvider.GetService(Type serviceType)
        {
            return this.GetService(serviceType);
        }
    }
}