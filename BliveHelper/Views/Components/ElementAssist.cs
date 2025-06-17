using System.Windows;
using System.Windows.Media;

namespace BliveHelper.Views.Components
{
    public static class ElementAssist
    {
        public static readonly DependencyProperty HoverBrushProperty = DependencyProperty.RegisterAttached(
            "HoverBrush",
            typeof(SolidColorBrush),
            typeof(ElementAssist),
            new PropertyMetadata(Brushes.Transparent)
        );

        public static SolidColorBrush GetHoverBrush(DependencyObject element)
        {
            return (SolidColorBrush)element.GetValue(HoverBrushProperty);
        }

        public static void SetHoverBrush(DependencyObject element, SolidColorBrush value)
        {
            element.SetValue(HoverBrushProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ElementAssist),
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
            typeof(ElementAssist),
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
