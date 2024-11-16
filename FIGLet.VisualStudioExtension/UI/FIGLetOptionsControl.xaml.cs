using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace FIGLet.VisualStudioExtension.UI;

/// <summary>
/// Interaction logic for FIGLetOptionsControl.xaml
/// </summary>
public partial class FIGLetOptionsControl : System.Windows.Controls.UserControl
{
    private readonly FIGLetOptions _options;
    private readonly List<FIGFontInfo> _fonts = [];
    private FIGFont _currentFont;

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetOptionsControl"/> class.
    /// </summary>
    /// <param name="options">The options page.</param>
    public FIGLetOptionsControl(FIGLetOptions options)
    {
        _options = options;
        InitializeComponent();

        MinWidth = 400;
        FontListView.ItemsSource = _fonts;

        // Initialize controls
        LayoutModeComboBox.Loaded += (s, e) =>
        {
            UpdateControls();
            UpdatePreview();
        };

        // Set default sample text
        SampleTextBox.Text = "Hello World";
        ThreadHelper.ThrowIfNotOnUIThread();
        PreviewTextBox.Foreground = ThemeHelper.GetCommentColorBrush(options);
        PreviewTextBox.Background = ThemeHelper.GetBackgroundColorBrush();
    }

    /// <summary>
    /// Updates the controls with the current settings.
    /// </summary>
    public void UpdateControls()
    {
        FontDirectoryTextBox.Text = _options.FontPath;
        LayoutModeComboBox.SelectedItem = _options.LayoutMode;

        LoadFonts();
    }

    /// <summary>
    /// Updates the settings based on the current control values.
    /// </summary>
    public void UpdateSettings()
    {
        _options.FontPath = FontDirectoryTextBox.Text;
        _options.LayoutMode = (LayoutMode)LayoutModeComboBox.SelectedItem;

        if (FontListView.SelectedItem is FIGFontInfo selectedFont)
            _options.LastSelectedFont = selectedFont.Name;
    }

    /// <summary>
    /// Updates the font count display.
    /// </summary>
    private void UpdateFontCount()
    {
        FontCountText.Text = $"Available Fonts ({_fonts.Count})";
    }

    /// <summary>
    /// Loads the fonts from the specified directory.
    /// </summary>
    private void LoadFonts()
    {
        FIGLetFontManager.SetFontDirectory(FontDirectoryTextBox.Text);
        _fonts.Clear();
        _fonts.AddRange(FIGLetFontManager.AvaliableFonts);
        UpdateFontCount();
    }

    /// <summary>
    /// Updates the preview text based on the current font and sample text.
    /// </summary>
    private void UpdatePreview()
    {
        if (PreviewTextBox == null)
            return;

        if (_currentFont == null || string.IsNullOrEmpty(SampleTextBox.Text))
        {
            PreviewTextBox.Text = string.Empty;
            return;
        }

        try
        {
            PreviewTextBox.Text = FIGLetRenderer.Render(
                SampleTextBox.Text,
                _currentFont,
                LayoutModeComboBox.SelectedItem == null ? LayoutMode.Default : (LayoutMode)LayoutModeComboBox.SelectedItem);
        }
        catch (Exception ex)
        {
            PreviewTextBox.Text = $"Error generating preview: {ex.Message}";
        }
    }

    /// <summary>
    /// Handles the Browse button click event to select a font directory.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.SelectedPath = FontDirectoryTextBox.Text;
            dialog.Description = "Select Folder Containing FIGLet Fonts";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FontDirectoryTextBox.Text = dialog.SelectedPath;
            }
        }
    }

    /// <summary>
    /// Handles the text changed event for the font directory text box.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void FontDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadFonts();
    }

    /// <summary>
    /// Handles the selection changed event for the font list view.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void FontListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedFont = FontListView.SelectedItem as FIGFontInfo;
        if (selectedFont != null)
        {
            try
            {
                _currentFont = selectedFont.Font;
                UpdatePreview();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error loading font: {ex.Message}",
                    "FIGLet Font Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Handles the selection changed event for the layout mode combo box.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void LayoutModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdatePreview();
    }

    /// <summary>
    /// Handles the text changed event for the sample text box.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void SampleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdatePreview();
    }
}