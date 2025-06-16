using BliveHelper.Utils.Structs;
using Newtonsoft.Json;

namespace BliveHelper.Utils
{
    public class WebSocketSetting : ObservableObject
    {
        private string serverUrl = "localhost:4455";
        [JsonProperty("server_url")]
        public string ServerUrl
        {
            get => serverUrl;
            set => SetProperty(ref serverUrl, value);
        }

        private string serverKey = string.Empty;
        [JsonProperty("server_key")]
        public string ServerKey
        {
            get => serverKey;
            set => SetProperty(ref serverKey, value);
        }
    }
}
