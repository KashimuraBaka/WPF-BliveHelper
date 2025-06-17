using System.Windows;
using System.Windows.Media;

namespace BliveHelper.Views.Components
{
    public static class ButtonAssist
    {
        public static readonly DependencyProperty HoverColorProperty = DependencyProperty.RegisterAttached(
            "HoverColor",
            typeof(SolidColorBrush),
            typeof(ButtonAssist),
            new PropertyMetadata(Brushes.Transparent)
        );

        public static SolidColorBrush GetHoverColor(DependencyObject element)
        {
            return (SolidColorBrush)element.GetValue(HoverColorProperty);
        }

        public static void SetHoverColor(DependencyObject element, SolidColorBrush value)
        {
            element.SetValue(HoverColorProperty, value);
        }
    }
}
