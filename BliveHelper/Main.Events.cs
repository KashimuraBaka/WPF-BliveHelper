using BilibiliDM_PluginFramework;
using BliveHelper.Utils;
using System;
using System.IO;

namespace BliveHelper
{
    public partial class Main
    {
        public override async void Inited()
        {
            Log("加载配置中...");
            if (!Directory.Exists(ENV.ConfigDirectory))
            {
                Log("未发现配置文件夹，尝试创建中");
                Directory.CreateDirectory(ENV.ConfigDirectory);
            }
            await ENV.Config.LoadAsync();
            // 尝试启动 WebSocket 服务
            ENV.StartWebSocket();
        }

        public override void DeInit()
        {
            AdminWindow.Close();
        }

        public override void Admin()
        {
            AdminWindow.ShowDialog();
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        private void OnConnected(object sender, ConnectedEvtArgs e)
        {
        }

        private void OnDisconnected(object sender, DisconnectEvtArgs e)
        {
        }

        private void OnReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
        }

        private void OnReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
        }

        private void OnError(object sender, UnhandledExceptionEventArgs e)
        {
            var obj = (Exception)e.ExceptionObject;
            ENV.Log(obj.ToString());
        }
    }
}
