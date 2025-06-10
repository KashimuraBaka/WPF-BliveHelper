using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Obs;
using BliveHelper.Utils.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BliveHelper.ViewModels.Windows
{
    public class MainWindowViewModel : ObservableObject
    {
        private Window Window { get; }
        private BliveAPI BliveAPI { get; } = new BliveAPI();
        private List<BliveArea> BaseLiveAreas { get; } = new List<BliveArea>();
        private long RefreshTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private ObsWebSocketAPI WebSocket { get; } = new ObsWebSocketAPI();

        // 扫码
        private bool scanQR;
        public bool ScanQR
        {
            get => scanQR;
            set => SetProperty(ref scanQR, value);
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
        // WebSocket 设置
        private string serverUrl = string.Empty;
        public string ServerUrl
        {
            get => serverUrl;
            set => SetProperty(ref serverUrl, value);
        }
        private string serverKey = string.Empty;
        public string ServerKey
        {
            get => serverKey;
            set => SetProperty(ref serverKey, value);
        }
        // 直播间信息
        private bool isStart;
        public bool IsStart
        {
            get => isStart;
            set => SetProperty(ref isStart, value);
        }
        private string userName = string.Empty;
        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }
        private int roomId;
        public int RoomId
        {
            get => roomId;
            set => SetProperty(ref roomId, value);
        }
        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        private string selectedArea = string.Empty;
        public string SelectedArea
        {
            get => selectedArea;
            set => SetProperty(ref selectedArea, value);
        }
        private string selectedGame = string.Empty;
        public string SelectedGame
        {
            get => selectedGame;
            set => SetProperty(ref selectedGame, value);
        }
        // 推流码
        private string streamServerUrl = string.Empty;
        public string StreamServerUrl
        {
            get => streamServerUrl;
            set => SetProperty(ref streamServerUrl, value);
        }
        private string streamServerKey = string.Empty;
        public string StreamServerKey
        {
            get => streamServerKey;
            set => SetProperty(ref streamServerKey, value);
        }
        private string broadcastCode = string.Empty;
        public string BroadcastCode
        {
            get => broadcastCode;
            set => SetProperty(ref broadcastCode, value);
        }
        // 弹幕信息
        private bool danmuEnable = true;
        public bool DanmuEnable
        {
            get => danmuEnable;
            set => SetProperty(ref danmuEnable, value);
        }
        private string danmuMessage = string.Empty;
        public string DanmuMessage
        {
            get => danmuMessage;
            set => SetProperty(ref danmuMessage, value);
        }
        // 命令
        public ICommand SignOutCommand => new RelayCommand(SignOut);
        public ICommand ActionLiveCommand => new RelayCommand(ActionLive);
        public ICommand SaveWebsocketSettingCommand => new RelayCommand(SaveWebsocketSetting);
        public ICommand SendDanmuCommand => new RelayCommand(SendDanmu);
        public ICommand ChangedSettingCommand => new RelayCommand(ChangedSetting);

        public ObservableCollection<string> LiveAreas { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> LiveGames { get; } = new ObservableCollection<string>();
        public bool ShowSignOutButton => !ScanQR;
        public string WebSocketConnectText => WebSocket.IsOpen ? "已连接" : "已断开";
        public string WebSocketVersionText => WebSocket.IsOpen ? $"[软件版本: {WebSocket.ObsStudioVerison}, 插件版本: {WebSocket.ObsPluginVersion}]" : string.Empty;
        public string WebSocketStateText => $"{WebSocketConnectText} {WebSocketVersionText}";
        public string UserStateText => string.IsNullOrEmpty(UserName) ? string.Empty : $"(已登录: {UserName})";
        public string RoomIdText => IsStart ? $"{RoomId} [正在直播]" : (RoomId > 0 ? RoomId.ToString() : "未登录");
        public string ActionButtonText => IsStart ? "停止直播" : "开始直播";
        public int GameAreaID => BaseLiveAreas.FirstOrDefault(x => x.Name == SelectedArea)?.List.FirstOrDefault(x => x.Name == SelectedGame)?.Id ?? 0;

        public MainWindowViewModel(Window window)
        {
            // 初始化窗口
            Window = window;
            // 监听属性变化
            PropertyChanged += MainWindowViewModel_PropertyChanged;
            // 监听 WebSokcet 事件
            WebSocket.OnStateChanged += WebSocket_OnStateChanged;
            // 初始化 WebSocket 设置
            ServerUrl = ENV.Config.WebSocket.ServerUrl;
            ServerKey = ENV.Config.WebSocket.ServerKey;
            WebSocket.Connect(ServerUrl, ServerKey);
            // 如果没有 Cookies 则显示二维码扫码登录
            if (ENV.Config.Cookies.Count == 0)
            {
                RefreshesQRCode();
            }
            else
            {
                BliveAPI.Cookies = ENV.Config.Cookies;
                Window.Dispatcher.Invoke(RefreshLiveInfo);
            }
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsStart):
                    NotifyPropertyChanged(nameof(RoomIdText));
                    NotifyPropertyChanged(nameof(ActionButtonText));
                    if (!IsStart)
                    {
                        StreamServerUrl = string.Empty;
                        StreamServerKey = string.Empty;
                        BroadcastCode = string.Empty;
                    }
                    break;
                case nameof(ScanQR):
                    NotifyPropertyChanged(nameof(ShowSignOutButton));
                    if (ScanQR) RefreshesQRCode();
                    break;
                case nameof(RoomId):
                    NotifyPropertyChanged(nameof(RoomIdText));
                    break;
                case nameof(SelectedArea):
                    // 切换分区时, 刷新游戏列表
                    if (!string.IsNullOrEmpty(SelectedArea))
                    {
                        LiveGames.Clear();
                        var area = BaseLiveAreas.FirstOrDefault(x => x.Name == SelectedArea);
                        if (area != null)
                        {
                            foreach (var game in area.List)
                            {
                                LiveGames.Add(game.Name);
                            }
                        }
                    }
                    break;
                case nameof(UserName):
                    NotifyPropertyChanged(nameof(UserStateText));
                    break;
            }
        }

        private void WebSocket_OnStateChanged(object sender, bool value)
        {
            NotifyPropertyChanged(nameof(WebSocketStateText));
        }

        private async Task ActionLive(object value)
        {
            if (!IsStart)
            {
                if (!string.IsNullOrEmpty(SelectedArea) && !string.IsNullOrEmpty(SelectedGame))
                {
                    if (await GetRtmpInfo())
                    {
                        IsStart = true;
                        WebSocket.StartStream();
                    }
                    else
                    {
                        MessageBox.Show("获取推流地址失败, 可能网络问题或者当前分区不支持推流码获取");
                    }
                }
                else
                {
                    MessageBox.Show("请选择直播分区");
                }
            }
            else
            {
                var res = await BliveAPI.StopLive(RoomId);
                if (res != null && res.Change == 1)
                {
                    IsStart = false;
                    WebSocket.StopStream();
                }
            }
        }

        private void SignOut(object _)
        {
            ENV.Config.Cookies = new Dictionary<string, string>();
            BliveAPI.Cookies = new Dictionary<string, string>();
            IsStart = false;
            ScanQR = true;
            UserName = string.Empty;
            RoomId = default;
            Title = string.Empty;
            SelectedArea = string.Empty;
            SelectedGame = string.Empty;
        }

        private void SaveWebsocketSetting(object _)
        {
            ENV.Config.WebSocket.ServerUrl = ServerUrl;
            ENV.Config.WebSocket.ServerKey = ServerKey;
            // 重连 WebSocket 服务
            WebSocket.Connect(ServerUrl, ServerKey);
        }

        private async void SendDanmu(object _)
        {
            DanmuEnable = false;
            if (RoomId > 0 && !string.IsNullOrEmpty(DanmuMessage) && await BliveAPI.SendDanmu(RoomId, DanmuMessage))
            {
                DanmuMessage = string.Empty;
            }
            DanmuEnable = true;
        }

        private async void ChangedSetting(object _)
        {
            if (!string.IsNullOrEmpty(SelectedArea) && !string.IsNullOrEmpty(SelectedGame))
            {
                var result = await BliveAPI.SetLiveInfo(RoomId, Title, GameAreaID);
                if (result.Success)
                {
                    MessageBox.Show("修改完毕!");
                }
                else
                {
                    MessageBox.Show(result.Message);
                }
            }
            else
            {
                MessageBox.Show("请选择直播分区");
            }
        }

        public async void RefreshesQRCode()
        {
            // 显示图形二维码
            ScanQR = true;
            // 获取二维码
            var imageData = await BliveAPI.GetLoginQRCodeImage();
            if (imageData != null)
            {
                QrCodeImage = imageData.Image;
                // 循环检查二维码状态
                while (imageData.Image == QrCodeImage)
                {
                    var state = await BliveAPI.PollLoginQRCode(imageData.Key);
                    if (state != null)
                    {
                        if (state.Code == 0)
                        {
                            ScanQR = false;
                            ENV.Config.Cookies = BliveAPI.Cookies;
                            RefreshLiveInfo();
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

        private async void RefreshLiveInfo()
        {
            // 添加直播分区
            BaseLiveAreas.Clear();
            BaseLiveAreas.AddRange(await BliveAPI.GetAreas());
            LiveAreas.Clear();
            foreach (var area in BaseLiveAreas)
            {
                LiveAreas.Add(area.Name);
            }
            // 获取直播间信息
            var liveInfo = await BliveAPI.GetInfo();
            Title = liveInfo?.Title ?? string.Empty;
            SelectedArea = liveInfo?.ParentName ?? string.Empty;
            SelectedGame = liveInfo?.AreaV2Name ?? string.Empty;
            // 监听直播间状态
            var customTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            RefreshTime = customTime;
            while (RefreshTime == customTime)
            {
                var info = await BliveAPI.GetInfo();
                if (info != null)
                {
                    RoomId = info.RoomId;
                    UserName = info.UserName;
                    // 如果当前未获取开播链接, 则进行获取开播链接
                    if (info.LiveStatus is BliveState.Live && !IsStart && await GetRtmpInfo())
                    {
                        IsStart = true;
                    }
                }
                await Task.Delay(1000);
            }
        }

        private async Task<bool> GetRtmpInfo()
        {
            // 设置直播间
            var rtmp = await BliveAPI.StartLive(RoomId, Title, GameAreaID);
            if (rtmp is null || string.IsNullOrEmpty(rtmp.ServerUrl))
            {
                return false;
            }
            StreamServerUrl = rtmp.ServerUrl;
            StreamServerKey = rtmp.Code;
            WebSocket.SetStreamServiceSettings(StreamServerUrl, StreamServerKey);
            // 获取身份码
            var broadcastCode = await BliveAPI.GetOperationOnBroadcastCode();
            if (string.IsNullOrEmpty(broadcastCode))
            {
                return false;
            }
            BroadcastCode = broadcastCode;

            return true;
        }
    }
}
