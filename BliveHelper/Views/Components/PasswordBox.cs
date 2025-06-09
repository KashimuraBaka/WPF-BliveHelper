using System.Windows;
using System.Windows.Controls;

namespace BliveHelper.Views.Components
{
    internal class PasswordBox : TextBox
    {
        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(PasswordBox),
            new FrameworkPropertyMetadata(default(string), OnPropertyChangedPassword)
        );

        private static void OnPropertyChangedPassword(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox && !passwordBox.IsKeyboardFocused)
            {
                passwordBox.Text = new string('●', passwordBox.Password.Length);
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Text = Password;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            Text = new string('●', Password.Length);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (IsKeyboardFocused)
            {
                Password = Text;
            }
        }
    }
}
