using Newtonsoft.Json;

namespace BliveHelper.Utils.Structs
{
    public enum DanmakuType
    {
        Normal = 0, // 普通弹幕
        Special = 1, // 特殊弹幕
        Gift = 2, // 礼物弹幕
        SuperChat = 3 // 超级聊天
    }


    public class Danmaku : ObservableObject
    {
        [JsonProperty("msg_id")]
        public string MessageId { get; set; }
        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }
        [JsonProperty("room_id")]
        public long RoomId { get; set; }
        // 此处设置数据绑定原因是 B 站修改协议位 open_id, 无法获取 uid. 需要后续反查获取
        private long userId;
        [JsonProperty("uid")]
        public long UserId
        {
            get => userId;
            set => SetProperty(ref userId, value);
        }
        [JsonProperty("uname")]
        public string UserName { get; set; }
        [JsonProperty("uface")]
        public string UserFace { get; set; }
        [JsonProperty("dm_type")]
        public DanmakuType DanmakuType { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public string MessageTime { get; set; }
        [JsonProperty("union_id")]
        public string UnionId { get; set; }
        [JsonProperty("open_id")]
        public string OpenId { get; set; }
        [JsonProperty("guard_level")]
        public int GuardLevel { get; set; }
        [JsonProperty("emoji_img_url")]
        public string EmojiImageUrl { get; set; }
        [JsonProperty("fans_medal_level")]
        public int FansMedalLevel { get; set; }
        [JsonProperty("fans_medal_name")]
        public string FansMedalName { get; set; }
        [JsonProperty("fans_medal_wearing_status")]
        public bool FansMedalWearingStatus { get; set; }
        [JsonProperty("glory_level")]
        public int GloryLevel { get; set; }
        [JsonProperty("reply_union_id")]
        public string ReplyUnionId { get; set; }
        [JsonProperty("reply_open_id")]
        public string ReplyOpenId { get; set; }
        [JsonProperty("reply_uname")]
        public string ReplyUserName { get; set; }
    }

    public class DanmakuRawData
    {
        [JsonProperty("cmd")]
        public string Command { get; set; }
        [JsonProperty("data")]
        public Danmaku Data { get; set; }
    }
}
