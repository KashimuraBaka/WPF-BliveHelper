using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BliveHelper.Utils.Obs
{
    public struct ObsMethodHandler
    {
        public TaskCompletionSource<object> Tcs { get; set; }
        public Type Type { get; set; }
        public ObsMethodHandler(TaskCompletionSource<object> tcs, Type type)
        {
            Tcs = tcs;
            Type = type;
        }
    }

    public struct ObsResponseMessage
    {
        public WebSocketMessageType Type { get; set; }
        public string Message { get; set; }
        public ObsResponseMessage(WebSocketMessageType type, string message)
        {
            Type = type;
            Message = message;
        }
    }

    public class ObsWebSocket : IDisposable
    {
        private SemaphoreSlim SemaphoreSlim { get; set; } = new SemaphoreSlim(1, 1);
        private ClientWebSocket WebSocket { get; set; } = new ClientWebSocket();
        private ConcurrentDictionary<string, ObsMethodHandler> ResponseMethods { get; } = new ConcurrentDictionary<string, ObsMethodHandler>();
        private string ServerKey { get; set; } = string.Empty;

        public int BufferSize { get; set; } = 2048;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;

        public WebSocketState State => WebSocket.State;

        public async void Connect(string url, string password)
        {
            // 存储密码信息
            ServerKey = password;
            try
            {
                // 执行连接
                await WebSocket.ConnectAsync(new Uri($"ws://{url}"), CancellationToken.None);
                // 异步执行循环任务
                ListenMessages();
            }
            catch
            {
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void ListenMessages()
        {
            var segment = new ArraySegment<byte>(new byte[BufferSize]);
            while (WebSocket != null && WebSocket.State is WebSocketState.Open)
            {
                var data = await ReadMessage(segment);
                // 消息终止
                if (data.Type != WebSocketMessageType.Close)
                {
                    // 处理消息
                    HandleMessage(data.Message);
                    continue;
                }
                break;
            }
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private async Task<ObsResponseMessage> ReadMessage(ArraySegment<byte> segment)
        {
            var strings = new StringBuilder();
            try
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await WebSocket.ReceiveAsync(segment, CancellationToken.None);
                    if (result.Count > 0)
                    {
                        strings.Append(Encoding.UTF8.GetString(segment.Array, 0, result.Count));
                    }
                } while (result.MessageType != WebSocketMessageType.Close && !result.EndOfMessage);
                return new ObsResponseMessage(result.MessageType, strings.ToString());
            }
            catch (Exception ex)
            {
                ENV.Log($"[WebSocket] 读取消息异常: {ex.Message}");
                return new ObsResponseMessage(WebSocketMessageType.Close, string.Empty);
            }
        }

        public void SendRequest<T>(ObsMessageTypes type, T data)
        {
            if (WebSocket != null && WebSocket.State is WebSocketState.Open)
            {
                var message = new ObsData<T>() { OperationCode = type, Data = data };
                var messageStr = JsonConvert.SerializeObject(message);
                var arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageStr));
                WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None).Wait();
            }
        }

        public Task<T> AsyncSendRequest<T>(string method) where T : class
        {
            return AsyncSendRequestByThread<T>(method, null);
        }

        public Task<T> AsyncSendRequest<T>(string method, object data, int timeout = 5) where T : class
        {
            return AsyncSendRequestByThread<T>(method, data, timeout);
        }

        private Task<T> AsyncSendRequestByThread<T>(string method, object data, int timeout = 5) where T : class
        {
            return Task.Run(async () =>
            {
                await SemaphoreSlim.WaitAsync();
                try
                {
                    if (WebSocket != null && WebSocket.State is WebSocketState.Open)
                    {
                        // 处理请求消息
                        var requestData = new ObsRequestMessage<object>() { RequestType = method, RequestData = data };
                        var message = new ObsData<ObsRequestMessage<object>>() { OperationCode = ObsMessageTypes.Request, Data = requestData };
                        var messageStr = JsonConvert.SerializeObject(message);
                        var arraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageStr));
                        // 发送后可以等待响应
                        var tcs = new TaskCompletionSource<object>();
                        var result = ResponseMethods.TryAdd(requestData.RequestId, new ObsMethodHandler(tcs, typeof(T)));
                        await WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
                        // 等待消息 
                        try
                        {
                            tcs.Task.Wait(timeout * 1000);
                            // 如果消息未被取消则处理响应事件
                            if (tcs.Task.IsCanceled)
                            {
                                return default;
                            }
                            return tcs.Task.Result as T;
                        }
                        catch
                        {
                            return default;
                        }
                    }
                    return default;
                }
                finally
                {
                    SemaphoreSlim?.Release();
                }
            });
        }

        private void HandleMessage(string message)
        {
            Task.Run(() =>
            {
                if (TryJsonDeserialize<ObsData<JObject>>(message, out var messageObject))
                {
                    switch (messageObject.OperationCode)
                    {
                        case ObsMessageTypes.Hello:
                            SendAuthentication(messageObject.Data.ToObject<ObsHelloResponse>());
                            break;
                        case ObsMessageTypes.Identified:
                            // 返回该消息说明连接成功
                            OnConnected?.Invoke(this, EventArgs.Empty);
                            break;
                        case ObsMessageTypes.RequestResponse:
                            var response = messageObject.Data.ToObject<ObsResponse<JObject>>();
                            if (response != null && ResponseMethods.TryRemove(response.RequestId, out var handler))
                            {
                                handler.Tcs.SetResult(response.ResponseData?.ToObject(handler.Type));
                            }
                            else
                            {
                                ENV.Log($"未知消息: {message}");
                            }
                            break;
                        case ObsMessageTypes.Event:
                            ENV.Log(message);
                            break;
                        default:
                            ENV.Log($"未知 OBS 响应代码: {messageObject.OperationCode}");
                            break;
                    }
                }
            });
        }

        private void SendAuthentication(ObsHelloResponse jsonObject)
        {
            var request = new ObsAuthenticationRequest();
            if (jsonObject != null)
            {
                var secret = HashEncode(ServerKey + jsonObject.Authentication.Salt);
                var authResponse = HashEncode(secret + jsonObject.Authentication.Challenge);
                request.Authentication = authResponse;
            }
            SendRequest(ObsMessageTypes.Identify, request);
        }

        private static bool TryJsonDeserialize<T>(string jsonStr, out T result) where T : class
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(jsonStr);
                return result != null;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private static string HashEncode(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var textBytes = Encoding.ASCII.GetBytes(input);
                var hash = sha256.ComputeHash(textBytes);
                return Convert.ToBase64String(hash);
            }
        }

        public void Dispose()
        {
            if (WebSocket != null && WebSocket.State is WebSocketState.Open)
            {
                WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                WebSocket.Dispose();
            }
            SemaphoreSlim.Dispose();
            SemaphoreSlim = null;
            GC.SuppressFinalize(this);
        }
    }
}
