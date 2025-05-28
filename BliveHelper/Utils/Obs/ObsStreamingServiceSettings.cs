using System.Text.Json.Serialization;

namespace BliveHelper.Utils.Obs
{
    public class ObsStreamSettingsData
    {
        [JsonPropertyName("streamServiceType")]
        public string Type { set; get; } = string.Empty;
        [JsonPropertyName("streamServiceSettings")]
        public ObsStreamSettings Settings { set; get; } = new();
    }

    public class ObsStreamSettings
    {
        [JsonPropertyName("bwtest")]
        public bool BwTest { set; get; }
        [JsonPropertyName("server")]
        public string ServerUrl { set; get; } = string.Empty;
        [JsonPropertyName("key")]
        public string ServerKey { set; get; } = string.Empty;
        [JsonPropertyName("use_auth")]
        public bool UseAuth { set; get; }
        [JsonPropertyName("username")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Username { set; get; }
        [JsonPropertyName("password")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Password { set; get; }
        [JsonPropertyName("service")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Service { get; set; }
        [JsonPropertyName("protocol")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Protocol { get; set; }
    }
}
