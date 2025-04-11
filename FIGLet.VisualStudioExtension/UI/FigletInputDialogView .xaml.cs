using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ByteForge.FIGLet.VisualStudioExtension.UI;

/// <summary>
/// Interaction logic for FIGLetInputDialogView.xaml
/// </summary>
public partial class FIGLetInputDialogView : UserControl
{
    private readonly AsyncPackage _package;

    /// <summary>
    /// Gets the selected font.
    /// </summary>
    public FIGFont SelectedFont { get; private set; }

    /// <summary>
    /// Gets or sets the input text from the text box.
    /// </summary>
    public string InputText
    {
        get => InputTextBox.Text;
        set => InputTextBox.Text = value;
    }

    /// <summary>
    /// Gets the dialog result indicating whether the user clicked OK or Cancel.
    /// </summary>
    public bool? DialogResult { get; private set; }

    /// <summary>
    /// Gets or sets the current language.
    /// </summary>
    /// <remarks>
    /// Language is used to determine the comment style for the preview text.
    /// </remarks>
    public string CurrentLanguage { get; set; }

    private readonly FIGLetOptions options;
    private FIGLetFontManager FontManager { get { return _lazyFontManager.Value; } }
    private readonly Lazy<FIGLetFontManager> _lazyFontManager = new(static () => new FIGLetFontManager(), true);

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetInputDialogView"/> class.
    /// </summary>
    /// <param name="package">The async package.</param>
    /// <param name="language">The programming language.</param>
    /// <param name="fileName">The name of the file being edited.</param>
    public FIGLetInputDialogView(AsyncPackage package, string language, string fileName)
    {
        InitializeComponent();
        _package = package;
        CurrentLanguage = language;

        if ((LanguageCommentStyles.SupportedLanguages.TryGetValue(language, out var langInfo) == false) && !string.IsNullOrEmpty(fileName))
        {
            /*
             * Try to get the file extension of the active document
             */
             var ext = Path.GetExtension(fileName).ToLowerInvariant().TrimStart('.');
            foreach (var lang in LanguageCommentStyles.SupportedLanguages)
                if (lang.Value.Extensions.Contains(ext))
                {
                    CurrentLanguage = lang.Key;
                    break;
                }
        }

        InputTextBox.TextChanged += (s, e) => UpdatePreview();
        InputTextBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
                OkButton_Click(this, e);
        };

        options = (FIGLetOptions)package.GetDialogPage(typeof(FIGLetOptions));

        InitializeFonts();

        PreviewBlock.Foreground = ThemeHelper.GetCommentColorBrush(_package);

        ThreadHelper.ThrowIfNotOnUIThread();
        ThemeHelper.ApplyVsThemeToButton(OkButton);
        ThemeHelper.ApplyVsThemeToButton(CancelButton);

        LanguageComboBox.Loaded += (s, e) =>
        {
            var items = LanguageCommentStyles.SupportedLanguages.OrderBy(x => x.Value.Name).Select(x => x.Value).ToList();
            LanguageComboBox.ItemsSource = items;
            LanguageComboBox.SelectedItem = items.FirstOrDefault(x => string.Compare(x.Key, CurrentLanguage, StringComparison.OrdinalIgnoreCase) == 0);
        };

        LanguageComboBox.SelectionChanged += (s, e) =>
        {
            if (LanguageComboBox.SelectedItem is ProgrammingLanguageInfo info)
            {
                CurrentLanguage = info.Key;
                UpdatePreview();
            }
        };

        LayoutModeComboBox.Loaded += (s, e) =>
        {
            LayoutModeComboBox.SelectedItem = options.LayoutMode;
            UpdatePreview();
        };
    }

    private void InitializeFonts()
    {
        // Load fonts from settings
        LoadFonts(options.FontPath);

        // Try to select the last used font
        if (!string.IsNullOrEmpty(options.LastSelectedFont))
        {
            var lastFont = FontComboBox.Items.Cast<FIGFontInfo>().FirstOrDefault(f => f.Name == options.LastSelectedFont);
            if (lastFont != null)
            {
                FontComboBox.SelectedItem = lastFont;
                SelectedFont = lastFont.Font;
            }
        }

        // Select the first font if no font is selected
        if (SelectedFont == null && FontComboBox.Items.Count > 0)
        {
            FontComboBox.SelectedIndex = 0;
            SelectedFont = ((FIGFontInfo)FontComboBox.SelectedItem).Font;
        }
    }

    /// <summary>
    /// Renders the preview text.
    /// </summary>
    /// <returns>The rendered preview text.</returns>
    private string RenderDefaultPreview()
    {
        var text = FIGLetRenderer.Render(text: "Hello, World!", font: SelectedFont, mode: (LayoutMode)(LayoutModeComboBox.SelectedItem ?? LayoutMode.Default));
        return LanguageCommentStyles.WrapInComments(text, CurrentLanguage);
    }

    /// <summary>
    /// Updates the preview text based on the input text.
    /// </summary>
    private void UpdatePreview()
    {
        OkButton.IsEnabled = InputTextBox.Text.Length > 0;
        if (string.IsNullOrWhiteSpace(InputTextBox.Text))
        {
            PreviewBlock.Text = RenderDefaultPreview();
            return;
        }

        var txt = FIGLetRenderer.Render(text: InputTextBox.Text, font: SelectedFont, mode: (LayoutMode)(LayoutModeComboBox.SelectedItem ?? LayoutMode.Default));
        PreviewBlock.Text = LanguageCommentStyles.WrapInComments(txt, CurrentLanguage);
    }

    /// <summary>
    /// Handles the click event of the OK button.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(InputTextBox.Text))
        {
            MessageBox.Show("Please enter some text to convert.", "Input Required",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
        options.SaveSettingsToStorage();
        Window.GetWindow(this)?.Close();
    }

    /// <summary>
    /// Handles the click event of the Cancel button.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Window.GetWindow(this)?.Close();
    }

    /// <summary>
    /// Loads the fonts from the specified directory.
    /// </summary>
    /// <param name="fontDirectory">The font directory.</param>
    private void LoadFonts(string fontDirectory)
    {
        FontManager.FontDirectory = fontDirectory;
        FontComboBox.Items.Clear();
        foreach (var itm in FontManager.AvailableFonts)
            FontComboBox.Items.Add(itm);
    }

    /// <summary>
    /// Handles the selection changed event for the font combo box.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FontComboBox.SelectedItem is FIGFontInfo selectedFont)
        {
            SelectedFont = selectedFont.Font;
            UpdatePreview();
            options.LastSelectedFont = selectedFont.Name;
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
        options.LayoutMode = (LayoutMode)LayoutModeComboBox.SelectedIndex;
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Hyperlink hlb)
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = hlb.NavigateUri.ToString(),
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Handle any exceptions that might occur
            MessageBox.Show($"Error opening URL: {ex.Message}");
        }
    }
}