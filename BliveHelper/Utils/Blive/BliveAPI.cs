﻿using QRCoder;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace BliveHelper.Utils.Blive
{
    internal class BliveAPI : IDisposable
    {
        private HttpClient Client { get; }
        private CookieContainer CookieContainer { get; } = new();
        public Dictionary<string, string> Cookies
        {
            get
            {
                var dict = new Dictionary<string, string>();
                foreach (Cookie cookie in CookieContainer.GetAllCookies())
                {
                    if (dict.TryGetValue(cookie.Domain, out var value))
                    {
                        value += $";{cookie.Name}={cookie.Value}";
                        dict[cookie.Domain] = value;
                    }
                    else
                    {
                        dict[cookie.Domain] = $"{cookie.Name}={cookie.Value}";
                    }
                }
                return dict;
            }
            set
            {
                // 标记所有 Cookie 过期
                foreach (Cookie cookie in CookieContainer.GetAllCookies())
                {
                    cookie.Expired = true;
                }
                // 更新 Cookie
                foreach (var (domain, cookies) in value)
                {
                    foreach (var cookieString in cookies.Split(";"))
                    {
                        var kv = cookieString.Split("=");
                        if (kv.Length == 2)
                        {
                            CookieContainer.Add(new Cookie(kv[0], kv[1], "/", domain));
                        }
                    }
                }
            }
        }
        public string UserID => GetCookie("DedeUserID");
        public string CSRF => GetCookie("bili_jct");

        public BliveAPI()
        {
            Client = new(new HttpClientHandler { CookieContainer = CookieContainer });
            //Client.DefaultRequestHeaders.Add("Origin", "https://link.bilibili.com");
            //Client.DefaultRequestHeaders.Referrer = new Uri("https://link.bilibili.com/p/center/index");
            Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
        }

        private async Task<BliveResponse<T>?> Get<T>(string url)
        {
            try
            {
                var response = await Client.GetAsync(url);
                return await response.Content.ReadFromJsonAsync<BliveResponse<T>>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching data from {url}: {ex.Message}");
                return default;
            }
        }

        private async Task<BliveResponse<T>?> PostFormUrlEncoded<T>(string url, object data)
        {
            var response = await Client.PostAsync(url, ToUrlEncodeed(data));
            return await response.Content.ReadFromJsonAsync<BliveResponse<T>>();
        }

        /// <summary>
        /// 访问Bilibili服务器检查二维码扫描后的登录状态
        /// </summary>
        public async Task<BliveQRCodeResponse?> GetLoginQRCode()
        {
            var response = await Get<BliveQRCodeResponse>("https://passport.bilibili.com/x/passport-login/web/qrcode/generate");
            return response?.Data;
        }

        /// <summary>
        /// 获取二维码图像
        /// </summary>
        /// <returns></returns>
        public async Task<(BitmapImage? Image, string key)> GetLoginQRCodeImage()
        {
            var qrcodeData = await GetLoginQRCode();
            if (qrcodeData is not null)
            {
                var qrGenerator = new QRCodeGenerator();
                var qrCode = new QRCode(qrGenerator.CreateQrCode(qrcodeData.Url, QRCodeGenerator.ECCLevel.Q));

                // 生成 Bitmap 图像
                using var qrBitmap = qrCode.GetGraphic(10);

                // 写入到 BitmapImage
                using var memory = new MemoryStream();
                qrBitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return (bitmapImage, qrcodeData.QRCodeKey);
            }
            return default;
        }

        /// <summary>
        /// 检查二维码状态
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<BliveQRCodePollResponse?> PollLoginQRCode(string key)
        {
            var response = await Get<BliveQRCodePollResponse>($"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={key}");
            return response?.Data ?? default;
        }

        /// <summary>
        /// 获取直播间房间号
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<string> GetRoomID(string userid)
        {
            var response = await Get<BliveRoomIDResponse>($"https://api.live.bilibili.com/room/v2/Room/room_id_by_uid?uid={userid}");
            return response?.Data?.RoomId.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 获取直播间分区
        /// </summary>
        /// <returns></returns>
        public async Task<BliveArea[]> GetAreas()
        {
            var response = await Get<BliveArea[]>("https://api.live.bilibili.com/room/v1/Area/getList?show_pinyin=1");
            return response?.Data ?? [];
        }

        /// <summary>
        /// 获取直播间ID
        /// </summary>
        /// <returns></returns>
        public async Task<BliveInfo?> GetInfo()
        {
            var response = await Get<BliveInfo>("https://api.live.bilibili.com/xlive/app-blink/v1/room/GetInfo?platform=android_link");
            return response?.Data;
        }

        /// <summary>
        /// 设置直播间标题
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<(bool Success, string Message, BliveTitleResponse? Data)> SetLiveInfo(int roomId, string name, int areaId)
        {
            var requestData = new BliveTitleRequestData()
            {
                RoomId = roomId,
                Title = name,
                AreaId = areaId,
                Csrf = CSRF,
                CsrfToken = CSRF,
            };
            var response = await PostFormUrlEncoded<BliveTitleResponse>("https://api.live.bilibili.com/room/v1/Room/update", requestData);
            return (response?.Code == 0, response?.Message ?? string.Empty, response?.Data);
        }

        public async Task<BliveRtmpInfo?> StartLive(int roomId, string name, int areaId)
        {
            // 设置直播间标题
            await SetLiveInfo(roomId, name, areaId);
            // 开始直播间
            var requestData = new BliveStartRequestData()
            {
                RoomId = roomId,
                AreaV2 = areaId,
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<BliveStartResponse>("https://api.live.bilibili.com/room/v1/Room/startLive", requestData);
            return response?.Data?.Rtmp;
        }

        public async Task<BliveStopResponse?> StopLive(int roomId)
        {
            // 开始直播间
            var requestData = new BliveStartRequestData()
            {
                RoomId = roomId,
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<BliveStopResponse>("https://api.live.bilibili.com/room/v1/Room/stopLive", requestData);
            return response?.Data;
        }

        public async Task<string> GetOperationOnBroadcastCode()
        {
            var requestData = new BliveOperationOnBroadcastCode()
            {
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<BliveOperationOnBroadcastCodeResponse>(
                "https://api.live.bilibili.com/xlive/open-platform/v1/common/operationOnBroadcastCode",
                requestData
            );
            return response?.Data?.Code ?? string.Empty;
        }

        public async Task<bool> SendDanmu(int roomId, string message)
        {
            var requestData = new BliveSendMessageRequest()
            {
                RoomId = roomId,
                Csrf = CSRF,
                CsrfToken = CSRF,
                Msg = message
            };
            var response = await PostFormUrlEncoded<JsonObject>("https://api.live.bilibili.com/msg/send", requestData);
            return response is not null && response.Code == 0;
        }

        private string GetCookie(string name)
        {
            var cookie = CookieContainer.GetAllCookies().FirstOrDefault(x => x.Name == name);
            return cookie?.Value ?? string.Empty;
        }

        private static FormUrlEncodedContent? ToUrlEncodeed(object obj)
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                var key = jsonAttr?.Name ?? prop.Name;
                dict.Add(key, prop.GetValue(obj)?.ToString() ?? string.Empty);
            }
            return new FormUrlEncodedContent(dict);
        }

        public void Dispose()
        {
            Client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
