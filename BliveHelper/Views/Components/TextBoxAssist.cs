using System.Windows;
using System.Windows.Media;

namespace BliveHelper.Views.Components
{
    public static class TextBoxAssist
    {
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(TextBoxAssist),
            new PropertyMetadata(default(CornerRadius))
        );

        public static CornerRadius GetCornerRadius(DependencyObject element)
        {
            return (CornerRadius)element.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject element, CornerRadius value)
        {
            element.SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty BorderHoverBrushProperty = DependencyProperty.RegisterAttached(
            "BorderHoverBrush",
            typeof(SolidColorBrush),
            typeof(TextBoxAssist),
            new PropertyMetadata(Brushes.Transparent)
        );

        public static SolidColorBrush GetBorderHoverBrush(DependencyObject element)
        {
            return (SolidColorBrush)element.GetValue(BorderHoverBrushProperty);
        }

        public static void SetBorderHoverBrush(DependencyObject element, SolidColorBrush value)
        {
            element.SetValue(BorderHoverBrushProperty, value);
        }
    }
}
