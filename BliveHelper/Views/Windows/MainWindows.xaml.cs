using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.ViewModels.Windows;
using BliveHelper.Views.Components;
using BliveHelper.Views.Pages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BliveHelper.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<TabItemModel> Pages
        {
            get => (ObservableCollection<TabItemModel>)GetValue(PagesProperty);
            set => SetValue(PagesProperty, value);
        }

        public static readonly DependencyProperty PagesProperty = DependencyProperty.Register(
            nameof(Pages),
            typeof(ObservableCollection<TabItemModel>),
            typeof(MainWindow),
            new PropertyMetadata(new ObservableCollection<TabItemModel>())
        );

        public TabItemModel SelectedPage
        {
            get => (TabItemModel)GetValue(SelectedPageProperty);
            set => SetValue(SelectedPageProperty, value);
        }

        public static readonly DependencyProperty SelectedPageProperty = DependencyProperty.Register(
            nameof(SelectedPage),
            typeof(TabItemModel),
            typeof(MainWindow),
            new PropertyMetadata(default(TabItemModel))
        );

        public MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)DataContext;
            set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();
            // 绑定事件
            Loaded += MainWindow_Loaded;
            // 添加标签页
            Pages.Add(new TabItemModel("基本信息", new LiveSettingsPage() { DataContext = ViewModel }));
            Pages.Add(new TabItemModel("OBS插件", new ObsSettingsPage()));
            SelectedPage = Pages.First();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 如果没有 Cookies 则显示二维码扫码登录
            if (ENV.Config.Cookies.Count == 0)
            {
                ViewModel.RefreshesQRCode();
            }
            else
            {
                ViewModel.RefreshLiveInfo();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void OnImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.RefreshesQRCode();
        }

        /// <summary>
        /// 弹幕姬插件测试类, 必须保留
        /// </summary>
        /// <param name="message"></param>
#pragma warning disable IDE1006 // 命名样式
        public void logging(string message)
#pragma warning restore IDE1006 // 命名样式
        {
            Debug.WriteLine(message);
        }
    }
}