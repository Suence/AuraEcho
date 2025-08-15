using System.Windows;
using System.Windows.Media;

namespace PowerLab.Themes.AttachedProperties
{
    public class ButtonBrush
    {
        public static readonly DependencyProperty ButtonPressBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonPressBackground",
                typeof(Brush),
                typeof(ButtonBrush),
                new PropertyMetadata(default(Brush)));
        public static void SetButtonPressBackground(
            DependencyObject element,
            Brush value)
            => element.SetValue(ButtonPressBackgroundProperty, value);
        public static Brush GetButtonPressBackground(
            DependencyObject element)
            => element.GetValue(ButtonPressBackgroundProperty) as Brush;

        public static readonly DependencyProperty ButtonMouseOverBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "ButtonMouseOverBackground",
                typeof(Brush),
                typeof(ButtonBrush),
                new PropertyMetadata(default(Brush)));
        public static void SetButtonMouseOverBackground(
            DependencyObject element,
            Brush value)
            => element.SetValue(ButtonMouseOverBackgroundProperty, value);
        public static Brush GetButtonMouseOverBackground(
            DependencyObject element)
            => element.GetValue(ButtonMouseOverBackgroundProperty) as Brush;
    }
}
