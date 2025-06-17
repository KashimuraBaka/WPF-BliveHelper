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
    }
}
