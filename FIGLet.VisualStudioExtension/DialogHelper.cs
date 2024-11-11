using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace FIGLet.VisualStudioExtension
{
    public static class DialogHelper
    {
        public static bool? ShowDialog(FIGLetInputDialogView content)
        {
            var window = new Window
            {
                Title = "FIGLet Comment Generator",
                Content = content,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                MaxWidth = (Screen.PrimaryScreen.Bounds.Width * 0.8),
                Owner = System.Windows.Application.Current.MainWindow,
            };

            window.Activated += (s, e) =>
            {
                if (window.MinWidth == 0)
                {
                    window.MinWidth = window.ActualWidth;
                    content.InputTextBox.Focus();
                }
            };

            window.SizeChanged += (s, e) =>
            {
                window.Top = (Screen.PrimaryScreen.Bounds.Height - window.ActualHeight) / 2;
                window.Left = (Screen.PrimaryScreen.Bounds.Width - window.ActualWidth) / 2;
            };

            window.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    window.DialogResult = false;
                    window.Close();
                }
            };
            
            window.ShowDialog();
            return content.DialogResult;
        }
    }
}
