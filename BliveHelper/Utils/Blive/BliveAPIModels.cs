using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace BliveHelper.Utils.Blive
{
    public enum BliveState : byte
    {
        NotLive = 0,
        Live,
        Loop
    }

    public class BliveQRCodeImage
    {
        public BitmapImage Image { get; set; }
        public string Key { get; set; }
        public BliveQRCodeImage(BitmapImage image, string key)
        {
            Image = image;
            Key = key;
        }
    }

    public struct SetLiveInfoResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public BliveTitleResponse Data { get; set; }
        public SetLiveInfoResult(bool success, string message, BliveTitleResponse data)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }

    public abstract class BliveCsrf
    {
        [JsonProperty("csrf")]
        public string Csrf { get; set; } = string.Empty;
        [JsonProperty("csrf_token")]
        public string CsrfToken { get; set; } = string.Empty;
    }

    public class BliveResponse<T>
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
        [JsonProperty("ttl")]
        public int TTL { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; } = default;
    }

    public class BliveQRCodeResponse
    {
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
        [JsonProperty("qrcode_key")]
        public string QRCodeKey { get; set; } = string.Empty;
    }

    public class BliveQRCodePollResponse
    {
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class BliveRoomIDResponse
    {
        [JsonProperty("room_id")]
        public long RoomId { get; set; }
    }

    public class BliveInfoData
    {
        [JsonProperty("room_id")]
        public int RoomId { get; set; }
        [JsonProperty("uid")]
        public int Uid { get; set; }
        [JsonProperty("uname")]
        public string UserName { get; set; } = string.Empty;
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;
        [JsonProperty("face")]
        public string Face { get; set; } = string.Empty;
        [JsonProperty("live_status")]
        public BliveState LiveStatus { get; set; }
        [JsonProperty("parent_id")]
        public int ParentId { get; set; }
        [JsonProperty("parent_name")]
        public string ParentName { get; set; } = string.Empty;
        [JsonProperty("area_v2_name")]
        public string AreaV2Name { get; set; } = string.Empty;
        [JsonProperty("area_v2_id")]
        public int AreaV2Id { get; set; }
        [JsonProperty("master_level")]
        public int MasterLevel { get; set; }
    }

    public class BliveArea
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("list")]
        public List<BliveGameAreaItem> List { get; set; } = new List<BliveGameAreaItem>();
    }

    public class BliveGameAreaItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("parent_id")]
        public int ParentId { get; set; }
        [JsonProperty("old_area_id")]
        public int OldAreaId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("act_id")]
        public int ActId { get; set; }
        [JsonProperty("pk_status")]
        public int PkStatus { get; set; }
        [JsonProperty("hot_status")]
        public int HotStatus { get; set; }
        [JsonProperty("lock_status")]
        public int LockStatus { get; set; }
        [JsonProperty("pic")]
        public string Pic { get; set; } = string.Empty;
        [JsonProperty("complex_area_name")]
        public string ComplexAreaName { get; set; } = string.Empty;
        [JsonProperty("pinyin")]
        public string Pinyin { get; set; } = string.Empty;
        [JsonProperty("parent_name")]
        public string ParentName { get; set; } = string.Empty;
        [JsonProperty("area_type")]
        public int AreaType { get; set; }
    }

    public class BliveStartRequestData : BliveCsrf
    {
        [JsonProperty("room_id")]
        public int RoomId { get; set; }
        [JsonProperty("platform")]
        public string Platform { get; set; } = "android_link";
        [JsonProperty("area_v2")]
        public int AreaV2 { get; set; }
        [JsonProperty("backup_stream")]
        public int BackupStream { get; set; }
    }

    public class BliveStopRequestData : BliveCsrf
    {
        [JsonProperty("room_id")]
        public int RoomId { get; set; }
        [JsonProperty("platform")]
        public string Platform { get; set; } = "android_link";
    }

    public class BliveTitleRequestData : BliveCsrf
    {
        [JsonProperty("room_id")]
        public int RoomId { get; set; }
        [JsonProperty("platform")]
        public string Platform { get; set; } = "android_link";
        [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title { get; set; } = string.Empty;
        [JsonProperty("area_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int AreaId { get; set; }
    }

    public class BliveTitleResponse
    {
        [JsonProperty("sub_session_key")]
        public string SubSessionKey { get; set; } = string.Empty;
        [JsonProperty("audit_info")]
        public BliveAuditInfo AuditInfo { get; set; } = new BliveAuditInfo();
    }

    public class BliveAuditInfo
    {
        [JsonProperty("audit_title_reason")]
        public string AuditTitleReason { get; set; } = string.Empty;
        [JsonProperty("update_title")]
        public string UpdateTitle { get; set; } = string.Empty;
        [JsonProperty("audit_title_status")]
        public int AuditTitleStatus { get; set; }
    }

    public class BliveStartResponse
    {
        [JsonProperty("change")]
        public int Change { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        [JsonProperty("rtmp")]
        public BliveRtmpInfo Rtmp { get; set; } = new BliveRtmpInfo();
    }

    public class BliveStopResponse
    {
        [JsonProperty("change")]
        public int Change { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class BliveRtmpInfo
    {
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Type { get; set; }
        [JsonProperty("addr")]
        public string ServerUrl { get; set; } = string.Empty;
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;
        [JsonProperty("new_link", NullValueHandling = NullValueHandling.Ignore)]
        public string NewLink { get; set; }
        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public string Provider { get; set; }
    }

    public class BliveOperationOnBroadcastCode : BliveCsrf
    {
        [JsonProperty("action")]
        public int Action { get; set; } = 1;
    }

    public class BliveOperationOnBroadcastCodeResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;
    }

    public class BliveSendMessageRequest : BliveCsrf
    {
        [JsonProperty("roomid")]
        public int RoomId { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; } = string.Empty;
        [JsonProperty("rnd")]
        public long Rnd { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        [JsonProperty("fontsize")]
        public int FontSize { get; set; } = 25;
        [JsonProperty("color")]
        public int Color { get; set; } = 16777215;
        [JsonProperty("mode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Mode { get; set; } = 1;
        [JsonProperty("bubble", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Bubble { get; set; }
        [JsonProperty("room_type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int RoomType { get; set; }
        [JsonProperty("jumpfrom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int JumpFrom { get; set; }
        [JsonProperty("reply_mid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ReplyMid { get; set; }
        [JsonProperty("reply_attr", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ReplyAttr { get; set; }
        [JsonProperty("reply_uname", NullValueHandling = NullValueHandling.Ignore)]
        public string ReplyUname { get; set; }
        [JsonProperty("replay_dmid", NullValueHandling = NullValueHandling.Ignore)]
        public string ReplayDmid { get; set; }
        [JsonProperty("statistics", NullValueHandling = NullValueHandling.Ignore)]
        public string Statistics { get; set; }
    }

    public class BliveStreamLine
    {
        [JsonProperty("cdn_name")]
        public string CdnName { get; set; } = string.Empty;
        [JsonProperty("checked")]
        public bool Checked { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("src")]
        public int Src { get; set; }
    }

    public class BliveStreamResponse
    {
        [JsonProperty("rtmp")]
        public BliveRtmpInfo Rtmp { get; set; } = new BliveRtmpInfo();
        [JsonProperty("stream_line")]
        public List<BliveStreamLine> StreamLine { get; set; } = new List<BliveStreamLine>();
    }

    public class BliveCoverInfo
    {
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("audit_status")]
        public bool AuditStatus { get; set; }
        [JsonProperty("audit_reason")]
        public string AuditReason { get; set; } = string.Empty;
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
        [JsonProperty("select_status")]
        public bool SelectStatus { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class BliveCoverRequestData : BliveCsrf
    {
        [JsonProperty("room_id")]
        public int RoomId { get; set; }
        [JsonProperty("pic_id")]
        public int PictureId { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
        [JsonProperty("type")]
        public string Type { get; set; } = "cover";
    }
}
