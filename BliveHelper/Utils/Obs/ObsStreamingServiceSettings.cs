using Newtonsoft.Json;

namespace BliveHelper.Utils.Obs
{
    public class ObsStreamSettingsData
    {
        [JsonProperty("streamServiceType")]
        public string Type { set; get; } = string.Empty;
        [JsonProperty("streamServiceSettings")]
        public ObsStreamSettings Settings { set; get; } = new ObsStreamSettings();
    }

    public class ObsStreamSettings
    {
        [JsonProperty("bwtest")]
        public bool BwTest { set; get; }
        [JsonProperty("server")]
        public string ServerUrl { set; get; } = string.Empty;
        [JsonProperty("key")]
        public string ServerKey { set; get; } = string.Empty;
        [JsonProperty("use_auth")]
        public bool UseAuth { set; get; }
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { set; get; }
        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { set; get; }
        [JsonProperty("service", NullValueHandling = NullValueHandling.Ignore)]
        public string Service { get; set; }
        [JsonProperty("protocol", NullValueHandling = NullValueHandling.Ignore)]
        public string Protocol { get; set; }
    }
}
