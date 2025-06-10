using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace BliveHelper.Utils.Obs
{
    public class ObsWebSocketAPI
    {
        private ObsWebSocket WebSocket { get; set; }
        private CancellationTokenSource Cts { get; set; }
        private string ServerUrl { get; set; } = string.Empty;
        private string ServerKey { get; set; } = string.Empty;

        public string ObsStudioVerison { get; private set; } = "Unknown";
        public string ObsPluginVersion { get; private set; } = "Unknown";
        public bool IsOpen => WebSocket?.State is WebSocketState.Open;

        public event EventHandler<bool> OnStateChanged;

        public void Connect(string url, string password)
        {
            ServerUrl = url;
            ServerKey = password;
            Task.Factory.StartNew(async () =>
            {
                // 创建 CancellationTokenSource, 防止重复循环
                var cts = new CancellationTokenSource();
                // 连接到 OBS WebSocket 服务器, 如果连接失败则尝试重连
                Cts?.Cancel();
                Cts = cts;
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (WebSocket != null)
                        {
                            WebSocket.OnConnected -= WebSocket_OnConnected;
                            WebSocket.OnDisconnected -= WebSocket_OnDisconnected;
                            WebSocket.Dispose();
                        }
                        WebSocket = new ObsWebSocket();
                        WebSocket.OnConnected += WebSocket_OnConnected;
                        WebSocket.OnDisconnected += WebSocket_OnDisconnected;
                        WebSocket.Connect(url, password);
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"连接 OBS WebSocket 失败: {e.Message}");
                        await Task.Delay(1000);
                    }
                }
            });
        }

        private void WebSocket_OnConnected(object sender, EventArgs e)
        {
            var obsVersion = GetVersion().Result;
            ObsStudioVerison = obsVersion?.OBSStudioVersion ?? "Unknown";
            ObsPluginVersion = obsVersion?.ObsPluginVersion ?? "Unknown";
            OnStateChanged?.Invoke(this, IsOpen);
        }

        private async void WebSocket_OnDisconnected(object sender, EventArgs e)
        {
            await Task.Delay(1000);
            Connect(ServerUrl, ServerKey);
            OnStateChanged?.Invoke(this, IsOpen);
        }

        public async Task<ObsVersion> GetVersion()
        {
            if (WebSocket != null)
            {
                return await WebSocket.AsyncSendRequest<ObsVersion>(nameof(GetVersion));
            }
            return default;
        }

        public async void StartStream()
        {
            if (WebSocket != null)
            {
                await WebSocket.AsyncSendRequest<ObsStreamSettingsData>(nameof(StartStream));
            }
        }

        public async void StopStream()
        {
            if (WebSocket != null)
            {
                await WebSocket.AsyncSendRequest<ObsStreamSettingsData>(nameof(StopStream));
            }
        }

        public async Task<ObsStreamSettingsData> GetStreamServiceSettings()
        {
            if (WebSocket != null)
            {
                return await WebSocket.AsyncSendRequest<ObsStreamSettingsData>(nameof(GetStreamServiceSettings));
            }
            return default;
        }

        public async void SetStreamServiceSettings(string serverUrl, string serverKey)
        {
            if (WebSocket != null)
            {
                var data = new ObsStreamSettingsData() { Type = "rtmp_custom" };
                data.Settings.ServerUrl = serverUrl;
                data.Settings.ServerKey = serverKey;
                await WebSocket.AsyncSendRequest<ObsStreamSettingsData>(nameof(SetStreamServiceSettings), data);
            }
        }
    }
}
