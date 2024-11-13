using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace FIGLet.VisualStudioExtension.UI
{
    public partial class FIGLetOptionsControl : System.Windows.Controls.UserControl
    {
        private readonly FIGLetOptions _optionsPage;
        private readonly ObservableCollection<FontInfo> _fonts = new();
        private FIGFont _currentFont;

        public FIGLetOptionsControl(FIGLetOptions optionsPage)
        {
            _optionsPage = optionsPage;
            InitializeComponent();

            FontListView.ItemsSource = _fonts;

            // Initialize controls
            UpdateControls();

            // Set default sample text
            SampleTextBox.Text = "Hello World";
            PreviewTextBox.Foreground = ThemeHelper.GetCommentColorBrush(optionsPage);
            PreviewTextBox.Background = ThemeHelper.GetBackgroundColorBrush();
        }

        public void UpdateControls()
        {
            FontDirectoryTextBox.Text = _optionsPage.FontPath;
            LayoutModeComboBox.SelectedItem = _optionsPage.LayoutMode;

            LoadFonts();
        }

        public void UpdateSettings()
        {
            _optionsPage.FontPath = FontDirectoryTextBox.Text;
            _optionsPage.LayoutMode = (LayoutMode)LayoutModeComboBox.SelectedItem;

            if (FontListView.SelectedItem is FontInfo selectedFont)
                _optionsPage.LastSelectedFont = selectedFont.Name;
        }

        private void UpdateFontCount()
        {
            FontCountText.Text = $"Available Fonts ({_fonts.Count})";
        }

        private void LoadFonts()
        {
            _fonts.Clear();
            UpdateFontCount();

            if (string.IsNullOrEmpty(FontDirectoryTextBox.Text) ||
                !Directory.Exists(FontDirectoryTextBox.Text))
            {
                return;
            }

            try
            {
                foreach (var file in Directory.GetFiles(FontDirectoryTextBox.Text, "*.flf"))
                {
                    try
                    {
                        _fonts.Add(new FontInfo(file));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading font {file}: {ex.Message}");
                    }
                }

                // Select the default font if it exists
                var defaultFont = _fonts.FirstOrDefault(f => f.Name == _optionsPage.LastSelectedFont);
                if (defaultFont != null)
                {
                    FontListView.SelectedItem = defaultFont;
                }

                UpdateFontCount();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error loading fonts: {ex.Message}",
                    "FIGLet Font Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

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
                    LayoutModeComboBox.SelectedItem == null ? LayoutMode.Smushing : (LayoutMode)LayoutModeComboBox.SelectedItem);
            }
            catch (Exception ex)
            {
                PreviewTextBox.Text = $"Error generating preview: {ex.Message}";
            }
        }

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

        private void FontDirectoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadFonts();
        }

        private void FontListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFont = FontListView.SelectedItem as FontInfo;
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

        private void LayoutModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void SampleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
        }
    }
}