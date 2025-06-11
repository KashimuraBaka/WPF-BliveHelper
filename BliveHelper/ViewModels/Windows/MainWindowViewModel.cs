using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
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
        private List<BliveArea> BaseLiveAreas { get; } = new List<BliveArea>();
        private long RefreshTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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
        private string title;
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
        private string danmuMessage = string.Empty;
        public string DanmuMessage
        {
            get => danmuMessage;
            set => SetProperty(ref danmuMessage, value);
        }
        // 控件属性
        private bool startEnable = true;
        public bool StartEnable
        {
            get => startEnable;
            set => SetProperty(ref startEnable, value);
        }
        private bool danmuEnable = true;
        public bool DanmuEnable
        {
            get => danmuEnable;
            set => SetProperty(ref danmuEnable, value);
        }
        // 命令
        public ICommand SignOutCommand => new RelayCommand(SignOut);
        public ICommand SendDanmuCommand => new RelayCommand(SendDanmu);
        public ICommand ActionLiveCommand => new RelayCommand(ActionLive);
        public ICommand ChangedSettingCommand => new RelayCommand(ChangedSetting);

        public ObservableCollection<string> LiveAreas { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> LiveGames { get; } = new ObservableCollection<string>();
        public bool ShowSignOutButton => !ScanQR;
        public string WebSocketConnectText => ENV.WebSocket.IsOpen ? "已连接" : "已断开";
        public string WebSocketVersionText => ENV.WebSocket.IsOpen ? $"[OBS版本: {ENV.WebSocket.ObsStudioVerison}, 插件版本: {ENV.WebSocket.ObsPluginVersion}]" : string.Empty;
        public string WebSocketStateText => $"{WebSocketConnectText} {WebSocketVersionText}";
        public string UserStateText => string.IsNullOrEmpty(UserName) ? string.Empty : $"(已登录: {UserName})";
        public string RoomIdText => IsStart ? $"{RoomId} [正在直播]" : (RoomId > 0 ? RoomId.ToString() : "未登录");
        public string ActionButtonText => IsStart ? "停止直播" : "开始直播";
        public int GameAreaID => BaseLiveAreas.FirstOrDefault(x => x.Name == SelectedArea)?.List.FirstOrDefault(x => x.Name == SelectedGame)?.Id ?? 0;

        public MainWindowViewModel()
        {
            // 监听属性变化
            PropertyChanged += MainWindowViewModel_PropertyChanged;
            // 监听 WebSokcet 事件
            ENV.WebSocket.OnStateChanged += WebSocket_OnStateChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsStart):
                    NotifyPropertyChanged(nameof(RoomIdText));
                    NotifyPropertyChanged(nameof(ActionButtonText));
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
            StartEnable = false;
            if (!IsStart)
            {
                if (!string.IsNullOrEmpty(SelectedArea) && !string.IsNullOrEmpty(SelectedGame))
                {
                    var rtmp = await ENV.BliveAPI.StartLive(RoomId, Title, GameAreaID);
                    if (rtmp != null && !string.IsNullOrEmpty(rtmp.ServerUrl))
                    {
                        IsStart = true;
                        StreamServerUrl = rtmp.ServerUrl;
                        StreamServerKey = rtmp.Code;
                        ENV.WebSocket.SetStreamServiceSettings(StreamServerUrl, StreamServerKey);
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
                var res = await ENV.BliveAPI.StopLive(RoomId);
                if (res != null && res.Change == 1)
                {
                    IsStart = false;
                    ENV.WebSocket.StopStream();
                }
            }
            StartEnable = true;
        }

        private void SignOut(object _)
        {
            var result = MessageBox.Show("确定要退出登录?", "注销", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result is MessageBoxResult.OK)
            {
                ENV.Config.Cookies = new Dictionary<string, string>();
                ENV.BliveAPI.Cookies = new Dictionary<string, string>();
                IsStart = false;
                ScanQR = true;
                UserName = string.Empty;
                RoomId = default;
                Title = string.Empty;
                SelectedArea = string.Empty;
                SelectedGame = string.Empty;
            }
        }

        private async void SendDanmu(object _)
        {
            DanmuEnable = false;
            if (RoomId > 0 && !string.IsNullOrEmpty(DanmuMessage) && await ENV.BliveAPI.SendDanmu(RoomId, DanmuMessage))
            {
                DanmuMessage = string.Empty;
            }
            DanmuEnable = true;
        }

        private async void ChangedSetting(object _)
        {
            if (!string.IsNullOrEmpty(SelectedArea) && !string.IsNullOrEmpty(SelectedGame))
            {
                var result = await ENV.BliveAPI.SetLiveInfo(RoomId, Title, GameAreaID);
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

        public async void RefreshLiveInfo()
        {
            // 添加直播分区
            BaseLiveAreas.Clear();
            BaseLiveAreas.AddRange(await ENV.BliveAPI.GetAreas());
            LiveAreas.Clear();
            foreach (var area in BaseLiveAreas)
            {
                LiveAreas.Add(area.Name);
            }
            // 获取直播间信息
            var liveInfo = await ENV.BliveAPI.GetInfo();
            Title = liveInfo?.Title ?? string.Empty;
            SelectedArea = liveInfo?.ParentName ?? string.Empty;
            SelectedGame = liveInfo?.AreaV2Name ?? string.Empty;
            // 监听直播间状态
            var customTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            RefreshTime = customTime;
            while (RefreshTime == customTime)
            {
                var info = await ENV.BliveAPI.GetInfo();
                if (info != null)
                {
                    IsStart = info.LiveStatus is BliveState.Live;
                    RoomId = info.RoomId;
                    UserName = info.UserName;
                    // 获取推流码信息
                    var streamInfo = await ENV.BliveAPI.GetLiveStremInfo(info.RoomId);
                    if (streamInfo != null)
                    {
                        StreamServerUrl = streamInfo.Rtmp.ServerUrl;
                        StreamServerKey = streamInfo.Rtmp.Code;
                    }
                    // 获取身份码信息
                    var broadcastCode = await ENV.BliveAPI.GetOperationOnBroadcastCode();
                    if (!string.IsNullOrEmpty(broadcastCode))
                    {
                        BroadcastCode = broadcastCode;
                    }
                }
                await Task.Delay(5000);
            }
        }
    }
}
