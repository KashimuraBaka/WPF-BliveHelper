using BliveHelper.Utils.QRCoder;
using BliveHelper.Utils.Structs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BliveHelper.Utils.Blive
{
    public class BliveAPI : IDisposable
    {
        private const string BLIVE_API_URL = "https://api.live.bilibili.com";

        private HttpClient Client { get; }
        private CookieContainer CookieContainer { get; } = new CookieContainer();
        public bool IsLogin => CookieContainer.Count > 0;
        public string UserID => GetCookie("DedeUserID");
        public string CSRF => GetCookie("bili_jct");
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
                foreach (var kv in value)
                {
                    foreach (var cookieString in kv.Value.Split(';'))
                    {
                        var cookieKv = cookieString.Split('=');
                        if (cookieKv.Length == 2)
                        {
                            CookieContainer.Add(new Cookie(cookieKv[0], cookieKv[1], "/", kv.Key));
                        }
                    }
                }
            }
        }

        public BliveAPI()
        {
            Client = new HttpClient(new HttpClientHandler { CookieContainer = CookieContainer })
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            //Client.DefaultRequestHeaders.Add("Origin", "https://link.bilibili.com");
            //Client.DefaultRequestHeaders.Referrer = new Uri("https://link.bilibili.com/p/center/index");
            Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
        }

        private async Task<BliveResponse<T>> Get<T>(string url)
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

        private async Task<BliveResponse<T>> PostFormUrlEncoded<T>(string url, object data)
        {
            try
            {
                var response = await Client.PostAsync(url, ToUrlEncoded(data));
                return await response.Content.ReadFromJsonAsync<BliveResponse<T>>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error posting data to {url}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// 访问Bilibili服务器检查二维码扫描后的登录状态
        /// </summary>
        public async Task<BliveQRCodeResponse> GetLoginQRCode()
        {
            var response = await Get<BliveQRCodeResponse>("https://passport.bilibili.com/x/passport-login/web/qrcode/generate");
            return response?.Data;
        }

        /// <summary>
        /// 获取二维码图像
        /// </summary>
        /// <returns></returns>
        public async Task<BliveQRCodeImage> GetLoginQRCodeImage()
        {
            var qrCodeResponse = await GetLoginQRCode();
            if (qrCodeResponse != null)
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(qrCodeResponse.Url, QRCodeGenerator.ECCLevel.Q);
                    var bitmapByteQRCode = new BitmapByteQRCode(qrCodeData);
                    var qrCodeBytes = bitmapByteQRCode.GetGraphic(20);
                    using (var ms = new MemoryStream(qrCodeBytes))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        return new BliveQRCodeImage(bitmapImage, qrCodeResponse.QRCodeKey);
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// 检查二维码状态
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<BliveQRCodePollResponse> PollLoginQRCode(string key)
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
            var response = await Get<BliveRoomIDResponse>($"{BLIVE_API_URL}/room/v2/Room/room_id_by_uid?uid={userid}");
            return response?.Data?.RoomId.ToString() ?? string.Empty;
        }

        /// <summary>
        /// 获取直播间分区
        /// </summary>
        /// <returns></returns>
        public async Task<BliveArea[]> GetAreas()
        {
            var response = await Get<BliveArea[]>($"{BLIVE_API_URL}/room/v1/Area/getList?show_pinyin=1");
            return response?.Data ?? Array.Empty<BliveArea>();
        }

        /// <summary>
        /// 获取直播间ID
        /// </summary>
        /// <returns></returns>
        public async Task<BliveInfoData> GetInfo()
        {
            var response = await Get<BliveInfoData>($"{BLIVE_API_URL}/xlive/app-blink/v1/room/GetInfo?platform=android_link");
            return response?.Data;
        }

        /// <summary>
        /// 直接获取 RTMP 流
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<BliveStreamResponse> GetLiveStremInfo(int roomId)
        {
            var response = await Get<BliveStreamResponse>($"{BLIVE_API_URL}/live_stream/v1/StreamList/get_stream_by_roomId?room_id={roomId}");
            return response?.Data;
        }

        /// <summary>
        /// 设置直播间标题
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<SetLiveInfoResult> SetLiveInfo(int roomId, string name, int areaId)
        {
            var requestData = new BliveTitleRequestData()
            {
                RoomId = roomId,
                Title = name,
                AreaId = areaId,
                Csrf = CSRF,
                CsrfToken = CSRF,
            };
            var response = await PostFormUrlEncoded<BliveTitleResponse>($"{BLIVE_API_URL}/room/v1/Room/update", requestData);
            return new SetLiveInfoResult(response?.Code == 0, response?.Message ?? string.Empty, response?.Data);
        }

        /// <summary>
        /// 获取直播封面
        /// </summary>
        /// <returns></returns>
        public async Task<BliveCoverInfo[]> GetLiveCovers()
        {
            var response = await Get<BliveCoverInfo[]>($"{BLIVE_API_URL}/room/v1/Cover/get_list");
            return response?.Data ?? Array.Empty<BliveCoverInfo>();
        }

        public async Task<string> ReplaceLiveCover(int roomId, int coverId, string coverUrl)
        {
            var requestData = new BliveCoverRequestData()
            {
                RoomId = roomId,
                PictureId = coverId,
                Url = coverUrl,
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<JObject[]>($"{BLIVE_API_URL}/room/v1/Cover/replace", requestData);
            return response?.Message ?? string.Empty;
        }

        /// <summary>
        /// 开始直播
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="name"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public async Task<BliveRtmpInfo> StartLive(int roomId, string name, int areaId)
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
            var response = await PostFormUrlEncoded<BliveStartResponse>($"{BLIVE_API_URL}/room/v1/Room/startLive", requestData);
            return response?.Data?.Rtmp;
        }

        /// <summary>
        /// 停止直播
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task<BliveStopResponse> StopLive(int roomId)
        {
            // 开始直播间
            var requestData = new BliveStartRequestData()
            {
                RoomId = roomId,
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<BliveStopResponse>($"{BLIVE_API_URL}/room/v1/Room/stopLive", requestData);
            return response?.Data;
        }

        /// <summary>
        /// 获取身份码
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetOperationOnBroadcastCode()
        {
            var requestData = new BliveOperationOnBroadcastCode()
            {
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<BliveOperationOnBroadcastCodeResponse>(
                $"{BLIVE_API_URL}/xlive/open-platform/v1/common/operationOnBroadcastCode",
                requestData
            );
            return response?.Data?.Code ?? string.Empty;
        }

        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> SendDanmu(int roomId, string message)
        {
            var requestData = new BliveSendMessageRequest()
            {
                RoomId = roomId,
                Csrf = CSRF,
                CsrfToken = CSRF,
                Msg = message
            };
            var response = await PostFormUrlEncoded<JObject>($"{BLIVE_API_URL}/msg/send", requestData);
            return response != null && response.Code == 0;
        }

        public async Task<string> AddBlockUser(int roomId, string userContent, int hour)
        {
            var requestData = new BliveBlockUserRequest()
            {
                RoomId = roomId,
                Block = hour > 0 ? 1 : 0,
                UserContent = userContent,
                Hour = hour,
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<object>($"{BLIVE_API_URL}/liveact/room_block_user", requestData);
            return response?.Message ?? string.Empty;
        }

        public async Task<string> RemoveBlockUser(int roomId, long blockId)
        {
            var requestData = new BliveRemoveBlockUserRequest()
            {
                RoomId = roomId,
                BlockId = blockId,
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<object>($"{BLIVE_API_URL}/banned_service/v1/Silent/del_room_block_user", requestData);
            return response?.Message ?? string.Empty;
        }

        public async Task<List<BliveBlockUserInfo>> GetBlockUsers(int roomId)
        {
            var currentPage = 1;
            var blockUsers = new List<BliveBlockUserInfo>();
            if (roomId > 0)
            {
                while (true)
                {
                    var response = await Get<BliveBlockUserInfo[]>($"{BLIVE_API_URL}/liveact/ajaxGetBlockList?roomid={roomId}&page={currentPage}");
                    if (response?.Data == null || response.Data.Count() == 0)
                    {
                        break;
                    }
                    blockUsers.AddRange(response.Data);
                    currentPage++;
                }
            }
            return blockUsers;
        }

        public async Task<BliveAdminResult> GetLiveAdmins(int roomId)
        {
            var currentPage = 1;
            var data = new BliveAdminResult() { Admins = new List<BliveAdminInfo>() };
            if (roomId > 0)
            {
                BliveResponse<BliveAdminPage> res;
                do
                {
                    res = await Get<BliveAdminPage>($"{BLIVE_API_URL}/xlive/web-room/v1/roomAdmin/get_by_room?roomid={roomId}&page_size=100&page={currentPage}");
                    if (res?.Data == null || res.Data.AdminList.Count() == 0)
                    {
                        break;
                    }
                    // 添加数据
                    data.MaxRoomAnchorsNumber = res.Data.MaxRoomAnchorsNumber;
                    data.Admins.AddRange(res.Data.AdminList);
                }
                while (++currentPage <= res.Data.Page.TotalPage);
            }
            return data;
        }

        public async Task<BliveAddAdminResult> AddLiveAdmin(string adminContent)
        {
            var requestData = new BliveAddAdminRequest()
            {
                Admin = adminContent,
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<BliveAddAdminResponse>($"{BLIVE_API_URL}/xlive/web-ucenter/v1/roomAdmin/appoint", requestData);
            return new BliveAddAdminResult(response?.Code == 0, response?.Data.UserInfo.UserId ?? 0, response?.Data.UserInfo.UserName);
        }

        public async Task<bool> RemoveLiveAdmin(long userId)
        {
            var requestData = new BliveRemoveAdminRequest()
            {
                UserId = userId,
                Csrf = CSRF,
                CsrfToken = CSRF
            };
            var response = await PostFormUrlEncoded<object>($"{BLIVE_API_URL}/xlive/app-ucenter/v1/roomAdmin/dismiss", requestData);
            return response != null && response.Code == 0;
        }

        public async Task<BiliUIDInfo[]> NameToUID(IEnumerable<string> names)
        {
            if (names.Count() > 0)
            {
                var namesParam = string.Join(",", names);
                var response = await Get<BiliUIDsResponse>($"https://api.bilibili.com/x/polymer/web-dynamic/v1/name-to-uid?names={namesParam}");
                return response?.Data?.UIDs ?? Array.Empty<BiliUIDInfo>();
            }
            return Array.Empty<BiliUIDInfo>();
        }

        private string GetCookie(string name)
        {
            foreach (Cookie cookie in CookieContainer.GetAllCookies())
            {
                if (cookie.Name == name)
                {
                    return cookie.Value;
                }
            }
            return string.Empty;
        }

        private static FormUrlEncodedContent ToUrlEncoded(object obj)
        {
            var dict = new Dictionary<string, string>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                var key = jsonAttr?.PropertyName ?? prop.Name;
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
