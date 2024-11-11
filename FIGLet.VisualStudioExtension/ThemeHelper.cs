﻿using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FIGLet.VisualStudioExtension
{
    /// <summary>
    /// Provides helper methods for applying Visual Studio themes to WPF controls.
    /// </summary>
    public static class ThemeHelper
    {
        /// <summary>
        /// Gets the themed color for the specified theme resource key.
        /// </summary>
        /// <param name="key">The theme resource key.</param>
        /// <returns>The color associated with the specified theme resource key.</returns>
        public static Color GetThemeColor(ThemeResourceKey key)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return VSColorTheme.GetThemedColor(key).ToMediaColor();
        }

        /// <summary>
        /// Gets the themed brush for the specified theme resource key.
        /// </summary>
        /// <param name="key">The theme resource key.</param>
        /// <returns>The brush associated with the specified theme resource key.</returns>
        public static Brush GetThemeBrush(ThemeResourceKey key)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return new SolidColorBrush(GetThemeColor(key));
        }

        /// <summary>
        /// Applies the Visual Studio theme to the specified button.
        /// </summary>
        /// <param name="button">The button to which the theme will be applied.</param>
        public static void ApplyVsThemeToButton(Button button)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var style = new Style(typeof(Button));

            // Normal state
            style.Setters.Add(new Setter(Button.BackgroundProperty, GetThemeBrush(CommonControlsColors.ButtonColorKey)));
            style.Setters.Add(new Setter(Button.ForegroundProperty, GetThemeBrush(CommonControlsColors.ButtonTextColorKey)));
            style.Setters.Add(new Setter(Button.BorderBrushProperty, GetThemeBrush(CommonControlsColors.ButtonBorderColorKey)));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(1)));

            // Mouse over trigger
            var mouseOverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.ForegroundProperty, GetThemeBrush(CommonControlsColors.ButtonHoverTextColorKey)));
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, GetThemeBrush(CommonControlsColors.ButtonHoverColorKey)));
            mouseOverTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, GetThemeBrush(CommonControlsColors.ButtonBorderHoverColorKey)));
            style.Triggers.Add(mouseOverTrigger);

            // Pressed trigger
            var pressedTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressedTrigger.Setters.Add(new Setter(Button.ForegroundProperty, GetThemeBrush(CommonControlsColors.ButtonPressedTextColorKey)));
            pressedTrigger.Setters.Add(new Setter(Button.BackgroundProperty, GetThemeBrush(CommonControlsColors.ButtonPressedColorKey)));
            pressedTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, GetThemeBrush(CommonControlsColors.ButtonBorderPressedColorKey)));
            style.Triggers.Add(pressedTrigger);

            // Disabled trigger
            var disabledTrigger = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            disabledTrigger.Setters.Add(new Setter(Button.BackgroundProperty, GetThemeBrush(CommonControlsColors.ButtonDisabledColorKey)));
            disabledTrigger.Setters.Add(new Setter(Button.ForegroundProperty, GetThemeBrush(CommonControlsColors.ButtonDisabledTextColorKey)));
            disabledTrigger.Setters.Add(new Setter(Button.BorderBrushProperty, GetThemeBrush(CommonControlsColors.ButtonBorderDefaultColorKey)));
            style.Triggers.Add(disabledTrigger);

            button.Style = style;
        }

        /// <summary>
        /// Gets the brush for comment text based on the current Visual Studio theme.
        /// </summary>
        /// <param name="package">The async package.</param>
        /// <returns>The brush used for comment text in the current theme.</returns>
        public static Brush GetCommentColorBrush(AsyncPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var componentModel = package.GetService<SComponentModel, IComponentModel>();
            if (componentModel == null) return Brushes.Green;

            var classificationFormatMapService = componentModel.GetService<IClassificationFormatMapService>();
            var formatMap = classificationFormatMapService.GetClassificationFormatMap("text");

            var classificationTypeRegistry = componentModel.GetService<IClassificationTypeRegistryService>();
            var commentType = classificationTypeRegistry.GetClassificationType("comment");

            var textProperties = formatMap.GetTextProperties(commentType);

            return (textProperties.ForegroundBrush as SolidColorBrush) ?? Brushes.Green;
        }

        /// <summary>
        /// Converts a System.Drawing.Color to a System.Windows.Media.Color.
        /// </summary>
        /// <param name="color">The System.Drawing.Color to convert.</param>
        /// <returns>The equivalent System.Windows.Media.Color.</returns>
        private static Color ToMediaColor(this System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}