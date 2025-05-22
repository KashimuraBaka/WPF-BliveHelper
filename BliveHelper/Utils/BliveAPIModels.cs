using System.Text.Json.Serialization;

namespace BliveHelper.Utils
{
    enum BliveState : byte
    {
        NotLive = 0,
        Live,
        Loop
    }

    public class BliveResponse<T>
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        [JsonPropertyName("ttl")]
        public int TTL { get; set; }
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    internal class BliveQRCodeResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        [JsonPropertyName("qrcode_key")]
        public string QRCodeKey { get; set; } = string.Empty;
    }

    internal class BliveQRCodePollResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    internal class BliveRoomIDResponse
    {
        [JsonPropertyName("room_id")]
        public long RoomId { get; set; }
    }

    internal class BliveInfo
    {
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }
        [JsonPropertyName("uid")]
        public int Uid { get; set; }
        [JsonPropertyName("uname")]
        public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("face")]
        public string Face { get; set; } = string.Empty;
        [JsonPropertyName("live_status")]
        public BliveState LiveStatus { get; set; }
        [JsonPropertyName("parent_id")]
        public int ParentId { get; set; }
        [JsonPropertyName("parent_name")]
        public string ParentName { get; set; } = string.Empty;
        [JsonPropertyName("area_v2_name")]
        public string AreaV2Name { get; set; } = string.Empty;
        [JsonPropertyName("area_v2_id")]
        public int AreaV2Id { get; set; }
        [JsonPropertyName("master_level")]
        public int MasterLevel { get; set; }
    }

    internal class BliveArea
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("list")]
        public BliveGameAreaItem[] List { get; set; } = [];
    }

    internal class BliveGameAreaItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("parent_id")]
        public int ParentId { get; set; }
        [JsonPropertyName("old_area_id")]
        public int OldAreaId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("act_id")]
        public int ActId { get; set; }
        [JsonPropertyName("pk_status")]
        public int PkStatus { get; set; }
        [JsonPropertyName("hot_status")]
        public int HotStatus { get; set; }
        [JsonPropertyName("lock_status")]
        public int LockStatus { get; set; }
        [JsonPropertyName("pic")]
        public string Pic { get; set; } = string.Empty;
        [JsonPropertyName("complex_area_name")]
        public string ComplexAreaName { get; set; } = string.Empty;
        [JsonPropertyName("pinyin")]
        public string Pinyin { get; set; } = string.Empty;
        [JsonPropertyName("parent_name")]
        public string ParentName { get; set; } = string.Empty;
        [JsonPropertyName("area_type")]
        public int AreaType { get; set; }
    }

    internal class BliveStartRequestData
    {
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }
        [JsonPropertyName("platform")]
        public string Platform { get; set; } = "android_link";
        [JsonPropertyName("area_v2")]
        public int AreaV2 { get; set; }
        [JsonPropertyName("backup_stream")]
        public int BackupStream { get; set; }
        [JsonPropertyName("csrf_token")]
        public string CsrfToken { get; set; } = string.Empty;
        [JsonPropertyName("csrf")]
        public string Csrf { get; set; } = string.Empty;
    }

    internal class BliveStopRequestData
    {
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }
        [JsonPropertyName("platform")]
        public string Platform { get; set; } = "android_link";
        [JsonPropertyName("csrf_token")]
        public string CsrfToken { get; set; } = string.Empty;
        [JsonPropertyName("csrf")]
        public string Csrf { get; set; } = string.Empty;
    }

    internal class BliveTitleRequestData
    {
        [JsonPropertyName("room_id")]
        public int RoomId { get; set; }
        [JsonPropertyName("platform")]
        public string Platform { get; set; } = "android_link";
        [JsonPropertyName("title")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("area_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int AreaId { get; set; }
        [JsonPropertyName("csrf_token")]
        public string CsrfToken { get; set; } = string.Empty;
        [JsonPropertyName("csrf")]
        public string Csrf { get; set; } = string.Empty;
    }

    internal class BliveTitleResponse
    {
        [JsonPropertyName("sub_session_key")]
        public string SubSessionKey { get; set; } = string.Empty;
        [JsonPropertyName("audit_info")]
        public BliveAuditInfo AuditInfo { get; set; } = new();
    }

    internal class BliveAuditInfo
    {
        [JsonPropertyName("audit_title_reason")]
        public string AuditTitleReason { get; set; } = string.Empty;
        [JsonPropertyName("update_title")]
        public string UpdateTitle { get; set; } = string.Empty;
        [JsonPropertyName("audit_title_status")]
        public int AuditTitleStatus { get; set; }
    }

    internal class BliveStartResponse
    {
        [JsonPropertyName("change")]
        public int Change { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("rtmp")]
        public BliveRtmpInfo Rtmp { get; set; } = new();
    }

    internal class BliveStopResponse
    {
        [JsonPropertyName("change")]
        public int Change { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    internal class BliveRtmpInfo
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }
        [JsonPropertyName("addr")]
        public string ServerUrl { get; set; } = string.Empty;
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
        [JsonPropertyName("new_link")]
        public string NewLink { get; set; } = string.Empty;
        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;
    }
}
