using System.Windows;
using System.Windows.Media;

namespace BliveHelper.Views.Components
{
    internal class PathAssist
    {

        public static readonly DependencyProperty DataProperty = DependencyProperty.RegisterAttached(
            "Data",
            typeof(Geometry),
            typeof(PathAssist),
            new PropertyMetadata(default(Geometry))
        );

        public static Geometry GetData(DependencyObject element)
        {
            return (Geometry)element.GetValue(DataProperty);
        }

        public static void SetData(DependencyObject element, Geometry value)
        {
            element.SetValue(DataProperty, value);
        }
    }
}
