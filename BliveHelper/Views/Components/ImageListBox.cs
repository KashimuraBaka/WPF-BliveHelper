using System.Windows;
using System.Windows.Controls;

namespace BliveHelper.Views.Components
{
    internal class ImageListBox : ListBox
    {
        public int ItemColumn
        {
            get => (int)GetValue(ItemColumnProperty);
            set => SetValue(ItemColumnProperty, value);
        }

        public static readonly DependencyProperty ItemColumnProperty = DependencyProperty.Register(
            nameof(ItemColumn),
            typeof(int),
            typeof(ImageListBox),
            new PropertyMetadata(1)
        );

        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }

        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(
            nameof(ItemMargin),
            typeof(Thickness),
            typeof(ImageListBox),
            new PropertyMetadata(new Thickness(0))
        );
    }
}
