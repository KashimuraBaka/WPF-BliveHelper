using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BliveHelper.Utils
{
    public partial class WebSocketSetting : ObservableObject
    {
        [ObservableProperty]
        [property: JsonPropertyName("server_url")]
        private string serverUrl = "localhost:4455";
        [ObservableProperty]
        [property: JsonPropertyName("server_key")]
        private string serverKey = string.Empty;
    }

    public partial class BliveSettingConfig : ObservableObject
    {
        [JsonIgnore]
        private JsonSerializerOptions JsonOptions { get; } = new() { WriteIndented = true };
        [JsonIgnore]
        private string FileName { get; set; } = string.Empty;

        [ObservableProperty]
        [property: JsonPropertyName("cookies")]
        private Dictionary<string, string> cookies = [];
        [ObservableProperty]
        [property: JsonPropertyName("websocket")]
        private WebSocketSetting webSocket = new();

        public BliveSettingConfig() { }

        public BliveSettingConfig(string fileName)
        {
            FileName = fileName;
            if (File.Exists(fileName))
            {
                using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs, Encoding.UTF8);
                var configString = sr.ReadToEnd();
                var config = JsonSerializer.Deserialize<BliveSettingConfig>(configString);
                if (config is not null)
                {
                    Cookies = config.Cookies;
                    WebSocket = config.webSocket;
                }
            }
            PropertyChanged += OnPropertyChanged;
            WebSocket.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                using var fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write);
                fs.SetLength(0);
                using var sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(JsonSerializer.Serialize(this, JsonOptions));
            }
        }
    }
}
