﻿using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Windows;
using System.Windows.Controls;

namespace FIGLet.VisualStudioExtension.UI;

/// <summary>
/// Interaction logic for FIGLetInputDialogView.xaml
/// </summary>
public partial class FIGLetInputDialogView : UserControl
{
    private string _language;
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

    private readonly FIGLetOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FIGLetInputDialogView"/> class.
    /// </summary>
    /// <param name="package">The async package.</param>
    /// <param name="language">The programming language.</param>
    public FIGLetInputDialogView(AsyncPackage package, string language)
    {
        InitializeComponent();
        _package = package;
        _language = language;

        InputTextBox.TextChanged += (s, e) => UpdatePreview();

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
            LanguageComboBox.SelectedItem = items.FirstOrDefault(x => string.Compare(x.Key, language, StringComparison.OrdinalIgnoreCase) == 0);

            items = LanguageCommentStyles.SupportedLanguages.Select(x => x.Value).Skip(51).OrderBy(x => x.Name).ToList();
            var rndr = new FIGLetRenderer(FIGFont.Default);
            for (var i = 0; i < items.Count; i++)
            {
                var txt = FIGLetRenderer.Render(items[i].Name, FIGFont.Default, (LayoutMode)(LayoutModeComboBox.SelectedItem ?? LayoutMode.Default));
                var comment = LanguageCommentStyles.WrapInComments(txt, items[i].Key);
                var enc = HtmlEncoder.Create(new TextEncoderSettings());
                Debug.WriteLine("<div class=\"language-item\">");
                Debug.WriteLine(items[i].Name);
                Debug.WriteLine($"<div class=\"tooltip\">{enc.Encode(comment)}</div>");
                Debug.WriteLine("</div>");
            }
        };

        LanguageComboBox.SelectionChanged += (s, e) =>
        {
            if (LanguageComboBox.SelectedItem is CommentInfo info)
            {
                _language = info.Key;
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
        var text = FIGLetRenderer.Render("Hello, World!", SelectedFont, (LayoutMode)(LayoutModeComboBox.SelectedItem ?? LayoutMode.Default));
        return LanguageCommentStyles.WrapInComments(text, _language);
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

        var txt = FIGLetRenderer.Render(InputTextBox.Text, SelectedFont, (LayoutMode)(LayoutModeComboBox.SelectedItem ?? LayoutMode.Default));
        PreviewBlock.Text = LanguageCommentStyles.WrapInComments(txt, _language);
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
        FIGLetFontManager.SetFontDirectory(fontDirectory);
        FontComboBox.Items.Clear();
        foreach (var itm in FIGLetFontManager.AvaliableFonts)
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

    }
}