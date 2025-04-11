using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace ByteForge.FIGLet.VisualStudioExtension.UI;

/// <summary>
/// Interaction logic for FIGLetOptionsControl.xaml
/// </summary>
public partial class FIGLetOptionsControl : System.Windows.Controls.UserControl
{
    private readonly FIGLetOptions _options;
    private FIGFont _currentFont;

    private FIGLetFontManager FontManager { get { return _lazyFontManager.Value; } }
    private readonly Lazy<FIGLetFontManager> _lazyFontManager = new(static () => new FIGLetFontManager(), true);

    public class LayoutModeItem
    {
        public LayoutMode Value { get; set; }
        public string DisplayName { get; set; }
    }

    private void InitializeLayoutModes()
    {
        var items = new List<LayoutModeItem>
        {
            new() { Value = LayoutMode.Default, DisplayName = "Default (Smushing)" },
            new() { Value = LayoutMode.Smushing, DisplayName = "Smushing" },
            new() { Value = LayoutMode.Kerning, DisplayName = "Kerning" },
            new() { Value = LayoutMode.FullSize, DisplayName = "Full Size" },
        };

        LayoutModeComboBox.ItemsSource = items;
        LayoutModeComboBox.DisplayMemberPath = "DisplayName";
        LayoutModeComboBox.SelectedValuePath = "Value";
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetOptionsControl"/> class.
    /// </summary>
    /// <param name="options">The options page.</param>
    public FIGLetOptionsControl(FIGLetOptions options)
    {
        _options = options;
        InitializeComponent();

        MinWidth = 400;
        FontListView.ItemsSource = FontManager.AvailableFonts;

        FontManager.FontsChanged += (s, e) =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            FontListView.ItemsSource = null;
            FontListView.ItemsSource = FontManager.AvailableFonts;
            UpdateFontCount();
        };

        // Initialize controls
        LayoutModeComboBox.Loaded += (s, e) =>
        {
            UpdateControls();
            UpdatePreview();
        };

        InitializeLayoutModes();

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
        if (FontDirectoryTextBox.Text != _options.FontPath)
        {
            FontDirectoryTextBox.Text = _options.FontPath;
        }
        LayoutModeComboBox.SelectedValue = _options.LayoutMode;
    }

    /// <summary>
    /// Updates the settings based on the current control values.
    /// </summary>
    public void UpdateSettings()
    {
        _options.FontPath = FontDirectoryTextBox.Text;
        _options.LayoutMode = (LayoutMode?)LayoutModeComboBox.SelectedValue ?? LayoutMode.Default;

        if (FontListView.SelectedItem is FIGFontInfo selectedFont)
            _options.LastSelectedFont = selectedFont.Name;
    }

    /// <summary>
    /// Updates the font count display.
    /// </summary>
    private void UpdateFontCount()
    {
        FontCountText.Text = $"Available Fonts ({FontManager.AvailableFonts.Count})";
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
                text: SampleTextBox.Text,
                font: _currentFont,
                mode: (LayoutMode)(LayoutModeComboBox.SelectedValue ?? LayoutMode.Default));
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
                FontManager.FontDirectory = FontDirectoryTextBox.Text = dialog.SelectedPath;
        }
    }

    /// <summary>
    /// Handles the text changed event for the font directory text box.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void FontDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FontManager.FontDirectory = FontDirectoryTextBox.Text;
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