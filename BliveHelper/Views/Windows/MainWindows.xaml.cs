using BliveHelper.ViewModels.Windows;
using System.Windows;

namespace BliveHelper.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel(this);
        }
        

        private void OnImageMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewModel.RefreshesQRCode();
        }
    }
}