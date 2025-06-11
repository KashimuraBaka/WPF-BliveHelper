using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BliveHelper.Utils.Blive
{
    public class BliveInfo : ObservableObject
    {
        #region 直播间信息
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
        private string selectedArea;
        public string SelectedArea
        {
            get => selectedArea;
            set => SetProperty(ref selectedArea, value);
        }
        private string selectedGame;
        public string SelectedGame
        {
            get => selectedGame;
            set => SetProperty(ref selectedGame, value);
        }
        #endregion

        #region 隐私密钥
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
        #endregion

        public BliveInfo()
        {
            AutoRefresh();
        }

        private async void AutoRefresh()
        {
            // 监听直播间状态
            while (true)
            {
                if (ENV.BliveAPI.IsLogin)
                {
                    var info = await ENV.BliveAPI.GetInfo();
                    if (info != null)
                    {
                        SelectedArea = SelectedArea ?? info.ParentName;
                        SelectedGame = SelectedGame ?? info.AreaV2Name;
                        Title = info.Title;
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
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
