using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace FIGLet.VisualStudioExtension.UI
{
    /// <summary>
    /// Interaction logic for FIGLetInputDialogView.xaml
    /// </summary>
    public partial class FIGLetInputDialogView : UserControl
    {
        private readonly FIGLetRenderer _renderer;
        private readonly string _language;

        private readonly string _defaultPreviewText;

        /// <summary>
        /// Gets the input text from the text box.
        /// </summary>
        public string InputText => InputTextBox.Text;

        /// <summary>
        /// Gets the dialog result indicating whether the user clicked OK or Cancel.
        /// </summary>
        public bool? DialogResult { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FIGLetInputDialogView"/> class.
        /// </summary>
        /// <param name="package">The async package.</param>
        /// <param name="renderer">The FIGLet renderer.</param>
        /// <param name="language">The programming language.</param>
        public FIGLetInputDialogView(AsyncPackage package, FIGLetRenderer renderer, string language)
        {
            InitializeComponent();
            _renderer = renderer;
            _language = language;

            _defaultPreviewText = LanguageCommentStyles.WrapInComments(_renderer.Render("Preview will appear here"), _language);

            InputTextBox.TextChanged += (s, e) => UpdatePreview();

            PreviewBlock.Text = _defaultPreviewText;
            PreviewBlock.Foreground = ThemeHelper.GetCommentColorBrush(package);

            ThemeHelper.ApplyVsThemeToButton(OkButton);
            ThemeHelper.ApplyVsThemeToButton(CancelButton);
        }

        /// <summary>
        /// Updates the preview text based on the input text.
        /// </summary>
        private void UpdatePreview()
        {
            OkButton.IsEnabled = InputTextBox.Text.Length > 0;
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                PreviewBlock.Text = _defaultPreviewText;
                return;
            }

            var txt = _renderer.Render(InputTextBox.Text);
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
    }
}