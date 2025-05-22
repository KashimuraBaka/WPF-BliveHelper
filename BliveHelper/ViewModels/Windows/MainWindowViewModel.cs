using BliveHelper.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BliveHelper.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private BliveAPI BliveAPI { get; } = new();
        private List<BliveArea> BaseLiveAreas { get; } = [];
        private long RefreshTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // 扫码
        [ObservableProperty] private bool scanQR;
        [ObservableProperty] private BitmapImage? qrCodeImage;
        [ObservableProperty] private string qrCodeMessage = string.Empty;
        // 直播间信息
        [ObservableProperty] private bool isStart;
        [ObservableProperty] private string userName = string.Empty;
        [ObservableProperty] private int roomId;
        [ObservableProperty] private string title = string.Empty;
        [ObservableProperty] private string selectedArea = string.Empty;
        [ObservableProperty] private string selectedGame = string.Empty;
        // 推流码
        [ObservableProperty] private string streamServerUrl = string.Empty;
        [ObservableProperty] private string streamCode = string.Empty;

        public BliveSettingConfig Config { get; } = new("config.json");
        public ObservableCollection<string> LiveAreas { get; } = [];
        public ObservableCollection<string> LiveGames { get; } = [];
        public bool ShowSignOutButton => !ScanQR;
        public string WindowTitleText => string.IsNullOrEmpty(UserName) ? "直播间助手 By: Kashimura" : $"直播间助手 (已登录: {UserName}) By: Kashimura";
        public string RoomIdText => IsStart ? $"{RoomId} [正在直播]" : (RoomId > 0 ? RoomId.ToString() : "未登录");
        public string ActionButtonText => IsStart ? "停止直播" : "开始直播";
        public int GameAreaID => BaseLiveAreas.FirstOrDefault(x => x.Name == SelectedArea)?.List.FirstOrDefault(x => x.Name == SelectedGame)?.Id ?? 0;

        public MainWindowViewModel()
        {
            // 生成二维码
            if (Config.Cookies.Count == 0)
            {
                RefreshesQRCode();
            }
            else
            {
                BliveAPI.Cookies = Config.Cookies;
                RefreshLiveInfo();
            }
        }

        partial void OnScanQRChanged(bool value)
        {
            OnPropertyChanged(nameof(ShowSignOutButton));
            if (value)
            {
                RefreshesQRCode();
            }
        }

        partial void OnIsStartChanged(bool value)
        {
            OnPropertyChanged(nameof(RoomIdText));
            OnPropertyChanged(nameof(ActionButtonText));
            if (!value)
            {
                StreamServerUrl = string.Empty;
                StreamCode = string.Empty;
            }
        }

        partial void OnSelectedAreaChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                LiveGames.Clear();
                var area = BaseLiveAreas.FirstOrDefault(x => x.Name == value);
                if (area is not null)
                {
                    foreach (var game in area.List)
                    {
                        LiveGames.Add(game.Name);
                    }
                }
            }
        }

        partial void OnUserNameChanged(string value)
        {
            OnPropertyChanged(nameof(WindowTitleText));
        }

        partial void OnRoomIdChanged(int value)
        {
            OnPropertyChanged(nameof(RoomIdText));
        }

        [RelayCommand]
        private async Task ActionLive()
        {
            if (!IsStart)
            {
                if (!string.IsNullOrEmpty(SelectedArea) && !string.IsNullOrEmpty(SelectedGame))
                {
                    if (await GetRtmpInfo())
                    {
                        IsStart = true;
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
                if (res is not null && res.Change == 1)
                {
                    IsStart = false;
                }
            }
        }

        [RelayCommand]
        private void SignOut()
        {
            Config.Cookies = [];
            BliveAPI.Cookies = [];
            IsStart = false;
            ScanQR = true;
            UserName = string.Empty;
            RoomId = default;
            Title = string.Empty;
            SelectedArea = string.Empty;
            SelectedGame = string.Empty;
        }

        [RelayCommand]
        private async Task ChangedSetting()
        {
            if (!string.IsNullOrEmpty(SelectedArea) && !string.IsNullOrEmpty(SelectedGame))
            {
                var (Success, Message, _) = await BliveAPI.SetLiveInfo(RoomId, Title, GameAreaID);
                if (Success)
                {
                    MessageBox.Show("修改完毕!");
                }
                else
                {
                    MessageBox.Show(Message);
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
            var (image, key) = await BliveAPI.GetLoginQRCodeImage();
            if (image is not null)
            {
                QrCodeImage = image;
                // 循环检查二维码状态
                while (image == QrCodeImage)
                {
                    var state = await BliveAPI.PollLoginQRCode(key);
                    if (state is not null)
                    {
                        if (state.Code == 0)
                        {
                            ScanQR = false;
                            Config.Cookies = BliveAPI.Cookies;
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
                if (info is not null)
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
            if (rtmp is not null && !string.IsNullOrEmpty(rtmp.ServerUrl))
            {
                StreamServerUrl = rtmp.ServerUrl;
                StreamCode = rtmp.Code;
                return true;
            }
            return false;
        }
    }
}
