using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Obs;
using BliveHelper.Utils.Structs;
using BliveHelper.Views.Components;
using BliveHelper.Views.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BliveHelper.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObsWebSocketAPI WebSocket => ENV.WebSocket;

        // 标签页
        public ObservableCollection<TabItemModel> Pages { get; } = new ObservableCollection<TabItemModel>();
        // 选择标签页
        private TabItemModel selectedPage;
        public TabItemModel SelectedPage
        {
            get => selectedPage;
            set => SetProperty(ref selectedPage, value);
        }
        // 直播信息
        public BliveInfo Info => ENV.BliveInfo;
        // 扫码
        private bool scanQR;
        public bool ScanQR
        {
            get => scanQR;
            set
            {
                SetProperty(ref scanQR, value);
                NotifyPropertyChanged(nameof(ShowSignOutButton));
            }
        }
        private BitmapImage qrCodeImage;
        public BitmapImage QrCodeImage
        {
            get => qrCodeImage;
            set => SetProperty(ref qrCodeImage, value);
        }
        private string qrCodeMessage = string.Empty;
        public string QrCodeMessage
        {
            get => qrCodeMessage;
            set => SetProperty(ref qrCodeMessage, value);
        }
        private string danmuMessage = string.Empty;
        public string DanmuMessage
        {
            get => danmuMessage;
            set => SetProperty(ref danmuMessage, value);
        }
        // 控件属性
        private bool danmuEnable = true;
        public bool DanmuEnable
        {
            get => danmuEnable;
            set => SetProperty(ref danmuEnable, value);
        }

        // 命令
        public ICommand SignOutCommand => new RelayCommand(SignOut);
        public ICommand SendDanmuCommand => new RelayCommand(SendDanmu);
        public ICommand OpenLivePageCommand => new RelayCommand(OpenLivePage);
        public ICommand CopyLiveRoomdIdCommand => new RelayCommand(CopyLiveRoomdId);

        public bool ShowSignOutButton => !ScanQR;
        public string WebSocketConnectText => WebSocket.IsOpen ? "已连接" : "已断开";
        public string WebSocketVersionText => WebSocket.IsOpen ? $"[OBS版本: {WebSocket.ObsStudioVerison}, 插件版本: {WebSocket.ObsPluginVersion}]" : string.Empty;
        public string WebSocketStateText => $"{WebSocketConnectText} {WebSocketVersionText}";
        public string UserStateText => string.IsNullOrEmpty(Info.UserName) ? string.Empty : $"(已登录: {Info.UserName}, UID: {Info.UserId})";
        public string RoomIdText => Info.IsStart ? $"{Info.RoomId} [正在直播]" : (Info.RoomId > 0 ? Info.RoomId.ToString() : "未登录");

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            // 绑定事件
            Loaded += MainWindow_Loaded;
            ENV.BliveInfo.PropertyChanged += BliveInfo_PropertyChanged;
            ENV.WebSocket.OnStateChanged += WebSocket_OnStateChanged;
            // 添加标签页
            Pages.Add(new TabItemModel("基本信息", new LiveSettingsPage()));
            Pages.Add(new TabItemModel("封面设置", new LiveCoverSettingsPage()));
            Pages.Add(new TabItemModel("用户封禁", new LiveBlockUsersPage()));
            Pages.Add(new TabItemModel("房管设置", new LiveAdminsPage()));
            Pages.Add(new TabItemModel("OBS插件", new ObsSettingsPage()));
            SelectedPage = Pages.First();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 如果没有 Cookies 则显示二维码扫码登录
            if (ENV.Config.Cookies.Count == 0)
            {
                RefreshesQRCode();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 取消关闭事件, 隐藏窗口
            e.Cancel = true;
            Hide();
        }

        private void OnQRImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 刷新二维码
            RefreshesQRCode();
        }

        private void BliveInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Info.IsStart):
                    NotifyPropertyChanged(nameof(RoomIdText));
                    break;
                case nameof(Info.RoomId):
                    NotifyPropertyChanged(nameof(RoomIdText));
                    break;
                case nameof(Info.UserName):
                    NotifyPropertyChanged(nameof(UserStateText));
                    break;
            }
        }

        private void WebSocket_OnStateChanged(object sender, bool value)
        {
            NotifyPropertyChanged(nameof(WebSocketStateText));
        }

        private void SignOut()
        {
            var result = MessageBox.Show("确定要退出登录?", "注销", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result is MessageBoxResult.OK)
            {
                ENV.Config.Cookies = new Dictionary<string, string>();
                ENV.BliveAPI.Cookies = new Dictionary<string, string>();
                Info.IsStart = false;
                ScanQR = true;
                Info.UserName = string.Empty;
                Info.RoomId = default;
                Info.Title = string.Empty;
                Info.SelectedArea = string.Empty;
                Info.SelectedGame = string.Empty;
                // 生成二维码
                RefreshesQRCode();
            }
        }

        private async void SendDanmu()
        {
            DanmuEnable = false;
            if (Info.RoomId > 0 && !string.IsNullOrEmpty(DanmuMessage) && await ENV.BliveAPI.SendDanmu(Info.RoomId, DanmuMessage))
            {
                DanmuMessage = string.Empty;
            }
            DanmuEnable = true;
        }

        private void OpenLivePage()
        {
            Process.Start(new ProcessStartInfo { FileName = $"https://live.bilibili.com/{Info.RoomId}", UseShellExecute = true });
        }

        private void CopyLiveRoomdId()
        {
            Clipboard.SetDataObject(Info.RoomId.ToString());
        }

        private async void RefreshesQRCode()
        {
            // 显示图形二维码
            ScanQR = true;
            // 获取二维码
            var imageData = await ENV.BliveAPI.GetLoginQRCodeImage();
            if (imageData != null)
            {
                QrCodeImage = imageData.Image;
                // 循环检查二维码状态
                while (imageData.Image == QrCodeImage)
                {
                    var state = await ENV.BliveAPI.PollLoginQRCode(imageData.Key);
                    if (state != null)
                    {
                        if (state.Code == 0)
                        {
                            ScanQR = false;
                            ENV.Config.Cookies = ENV.BliveAPI.Cookies;
                            break;
                        }
                        else if (state.Code == 86038)
                        {
                            QrCodeMessage = "二维码失效, 重新生成中";
                            RefreshesQRCode();
                            break;
                        }
                        else if (state.Code == 86090)
                        {
                            QrCodeMessage = "二维码已扫描，等待确认";
                        }
                        else
                        {
                            QrCodeMessage = "等待扫描中";
                        }
                    }
                    await Task.Delay(1000);
                }
            }
        }

        #region MVVM 辅助方法
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                NotifyPropertyChanged(propertyName);
            }
        }
        #endregion

        #region 弹幕姬插件, 必备
        /// <param name="message"></param>
#pragma warning disable IDE1006 // 命名样式
        public void logging(string message)
#pragma warning restore IDE1006 // 命名样式
        {
            Debug.WriteLine(message);
        }
        #endregion
    }
}