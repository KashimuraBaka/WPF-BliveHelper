using BliveHelper.Utils.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BliveHelper.Utils.Blive
{
    public class BliveInfo : ObservableObject
    {
        private bool FirstLoad { get; set; } = true;

        #region 直播间信息
        private bool isStart;
        public bool IsStart
        {
            get => isStart;
            set => SetProperty(ref isStart, value);
        }
        private long userId;
        public long UserId
        {
            get => userId;
            set => SetProperty(ref userId, value);
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
        private string selectedArea;
        public string SelectedArea
        {
            get => selectedArea;
            set
            {
                SetProperty(ref selectedArea, value);
                NotifyPropertyChanged(nameof(LiveGames));
                if (!FirstLoad) SelectedGame = LiveGames.FirstOrDefault()?.Name;
            }
        }
        private string selectedGame;
        public string SelectedGame
        {
            get => selectedGame;
            set => SetProperty(ref selectedGame, value);
        }
        private string news;
        public string News
        {
            get => news;
            set => SetProperty(ref news, value);
        }
        #endregion

        #region 隐私密钥
        private string streamServerUrl;
        public string StreamServerUrl
        {
            get => streamServerUrl;
            set => SetProperty(ref streamServerUrl, value);
        }
        private string streamServerKey;
        public string StreamServerKey
        {
            get => streamServerKey;
            set => SetProperty(ref streamServerKey, value);
        }
        private string broadcastCode;
        public string BroadcastCode
        {
            get => broadcastCode;
            set => SetProperty(ref broadcastCode, value);
        }
        #endregion

        public int GameAreaID => LiveAreas.FirstOrDefault(x => x.Name == SelectedArea)?.List.FirstOrDefault(x => x.Name == SelectedGame)?.Id ?? 0;
        public ObservableCollection<BliveArea> LiveAreas { get; } = new ObservableCollection<BliveArea>();
        public IEnumerable<BliveGameAreaItem> LiveGames => LiveAreas.FirstOrDefault(x => x.Name == SelectedArea)?.List ?? Enumerable.Empty<BliveGameAreaItem>();

        public event EventHandler OnInfoRefreshed;

        public BliveInfo()
        {
            AutoRefresh();
        }

        private async void AutoRefresh()
        {
            // 获取直播分区
            LiveAreas.Clear();
            LiveAreas.AddRange(await ENV.BliveAPI.GetAreas());
            // 监听直播间状态
            while (true)
            {
                if (ENV.BliveAPI.IsLogin)
                {
                    var info = await ENV.BliveAPI.GetInfo();
                    if (info != null)
                    {
                        UserId = info.UserId;
                        UserName = info.UserName;
                        IsStart = info.LiveStatus is BliveState.Live;
                        RoomId = info.RoomId;
                        if (FirstLoad)
                        {
                            SelectedArea = info.ParentName;
                            SelectedGame = info.AreaV2Name;
                            Title = info.Title;
                            News = info.AnchorContent;
                            FirstLoad = false;
                        }
                        // 获取推流码信息
                        if (StreamServerUrl is null || StreamServerKey is null)
                        {
                            var streamInfo = await ENV.BliveAPI.GetLiveStremInfo(info.RoomId);
                            if (streamInfo != null)
                            {
                                StreamServerUrl = streamInfo.Rtmp.ServerUrl;
                                StreamServerKey = streamInfo.Rtmp.Code;
                            }
                        }
                        // 获取身份码信息
                        BroadcastCode = BroadcastCode ?? await ENV.BliveAPI.GetOperationOnBroadcastCode();
                        // 刷新事件
                        OnInfoRefreshed?.Invoke(this, EventArgs.Empty);
                    }
                    await Task.Delay(5000);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        public async Task<string> ToggleStreamLive()
        {
            return IsStart ? await StopStreamLive() : await StartStreamLive();
        }

        public async Task<string> StartStreamLive()
        {
            if (!string.IsNullOrEmpty(SelectedArea) && !string.IsNullOrEmpty(SelectedGame))
            {
                var news_result = await ENV.BliveAPI.UpdateLiveNews(RoomId, UserId, News);
                var rtmp_result = await ENV.BliveAPI.StartLive(RoomId, Title, GameAreaID);
                if (news_result && rtmp_result != null && !string.IsNullOrEmpty(rtmp_result.Data.Rtmp.ServerUrl))
                {
                    IsStart = true;
                    StreamServerUrl = rtmp_result.Data.Rtmp.ServerUrl;
                    StreamServerKey = rtmp_result.Data.Rtmp.Code;
                    await ENV.WebSocket.SetStreamServiceSettings(StreamServerUrl, StreamServerKey);
                    await ENV.WebSocket.StartStream();
                    return string.Empty;
                }
                else if (rtmp_result.Code == 60024)
                {
                    ENV.Plugin.AdminWindow.ShowQrCode(
                        rtmp_result.Data.QRCode,
                        "当前分区需要进行人脸认证, 请通过手机客户端扫描进行操作\n(完成操作后可关闭当前二维码)",
                        showClose: true
                    );
                    return string.Empty;
                }
                else
                {
                    return $"获取推流地址失败, 具体错误:\n{rtmp_result.Message}";
                }
            }
            else
            {
                return "请选择直播分区";
            }
        }

        public async Task<string> StopStreamLive()
        {
            var res = await ENV.BliveAPI.StopLive(RoomId);
            if (res != null && res.Change == 1)
            {
                IsStart = false;
                await ENV.WebSocket.StopStream();
            }
            return string.Empty;
        }

        public async Task<string> SaveSetting()
        {
            if (!string.IsNullOrEmpty(SelectedArea) && !string.IsNullOrEmpty(SelectedGame))
            {
                var info_result = await ENV.BliveAPI.SetLiveInfo(RoomId, Title, GameAreaID);
                var news_result = await ENV.BliveAPI.UpdateLiveNews(RoomId, UserId, News);
                return info_result.Success && news_result ? "修改完毕!" : info_result.Message;
            }
            return "请选择直播分区";
        }
    }
}
