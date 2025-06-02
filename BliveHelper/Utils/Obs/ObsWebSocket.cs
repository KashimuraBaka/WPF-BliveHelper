using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BliveHelper.Utils.Obs
{
    public class ObsWebSocket : IDisposable
    {
        private record struct ObsMethodHandler(TaskCompletionSource<object?> Tcs, Type Type);

        private ClientWebSocket WebSocket { get; set; } = new();
        private ConcurrentDictionary<string, ObsMethodHandler> ResponseMethods { get; } = new();
        private string ServerKey { get; set; } = string.Empty;

        public int BufferSize { get; set; } = 2048;

        public event EventHandler? OnConnected;
        public event EventHandler? OnDisconnected;

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
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var segment = new ArraySegment<byte>(buffer);
                while (WebSocket is not null && WebSocket.State is WebSocketState.Open)
                {
                    var (messageType, message) = await ReadMessage(segment);
                    // 消息终止
                    if (messageType is not WebSocketMessageType.Close)
                    {
                        // 处理消息
                        HandleMessage(message);
                        continue;
                    }
                    break;
                }
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private async Task<(WebSocketMessageType type, string message)> ReadMessage(ArraySegment<byte> segment)
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
                        strings.Append(Encoding.UTF8.GetString(segment[..result.Count]));
                    }
                } while (result.MessageType is not WebSocketMessageType.Close && !result.EndOfMessage);
                return (result.MessageType, strings.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error] 读取消息异常: {ex.Message}");
                return (WebSocketMessageType.Close, string.Empty);
            }
        }

        public void SendRequest<T>(ObsMessageTypes type, T? data)
        {
            if (WebSocket is not null && WebSocket.State is WebSocketState.Open)
            {
                var message = new ObsData<T>() { OperationCode = type, Data = data };
                var messageStr = JsonSerializer.Serialize(message);
                WebSocket.SendAsync(Encoding.UTF8.GetBytes(messageStr), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None).Wait();
            }
        }

        public async Task<T?> AsyncSendRequest<T>(string method) where T : class
        {
            return await AsyncSendRequest<T>(method, null);
        }

        public async Task<T?> AsyncSendRequest<T>(string method, object? data, int timeout = 5) where T : class
        {
            if (WebSocket is not null && WebSocket.State is WebSocketState.Open)
            {
                // 处理请求消息
                var requestData = new ObsRequestMessage<object>() { RequestType = method, RequestData = data };
                var message = new ObsData<ObsRequestMessage<object>>() { OperationCode = ObsMessageTypes.Request, Data = requestData };
                var messageStr = JsonSerializer.Serialize(message);
                await WebSocket.SendAsync(Encoding.UTF8.GetBytes(messageStr), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
                // 发送后可以等待响应
                var tcs = new TaskCompletionSource<object?>();
                ResponseMethods.TryAdd(requestData.RequestId, new ObsMethodHandler(tcs, typeof(T)));
                // 等待消息 
                try
                {
                    await tcs.Task.WaitAsync(TimeSpan.FromSeconds(timeout));
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

        private void HandleMessage(string message)
        {
            Task.Run(() =>
            {
                if (TryJsonDeserialize<ObsData<JsonObject>>(message, out var messageObject))
                {
                    switch (messageObject.OperationCode)
                    {
                        case ObsMessageTypes.Hello:
                            SendAuthentication(messageObject.Data.Deserialize<ObsHelloResponse>());
                            break;
                        case ObsMessageTypes.Identified:
                            // 返回该消息说明连接成功
                            OnConnected?.Invoke(this, EventArgs.Empty);
                            break;
                        case ObsMessageTypes.RequestResponse:
                            var response = messageObject.Data.Deserialize<ObsResponse<JsonObject>>();
                            if (response is not null && ResponseMethods.TryRemove(response.RequestId, out var handler))
                            {
                                handler.Tcs.SetResult(response.ResponseData?.Deserialize(handler.Type));
                            }
                            else
                            {
                                Debug.WriteLine($"Unknown Message: {message}");
                            }
                            break;
                        case ObsMessageTypes.Event:
                            Debug.WriteLine(message);
                            break;
                        default:
                            Debug.WriteLine($"Unknown Code: {messageObject.OperationCode}");
                            break;
                    }
                }
            });
        }

        private void SendAuthentication(ObsHelloResponse? jsonObject)
        {
            var request = new ObsAuthenticationRequest();
            if (jsonObject is not null)
            {
                var secret = HashEncode(ServerKey + jsonObject.Authentication.Salt);
                var authResponse = HashEncode(secret + jsonObject.Authentication.Challenge);
                request.Authentication = authResponse;
            }
            SendRequest(ObsMessageTypes.Identify, request);
        }

        private static bool TryJsonDeserialize<T>(string jsonStr, [NotNullWhen(true)] out T? result) where T : class
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(jsonStr);
                return result is not null;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        private static string HashEncode(string input)
        {
            var textBytes = Encoding.ASCII.GetBytes(input);
            var hash = SHA256.HashData(textBytes);
            return Convert.ToBase64String(hash);
        }

        public void Dispose()
        {
            if (WebSocket is not null && WebSocket.State is WebSocketState.Open)
            {
                WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                WebSocket.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
