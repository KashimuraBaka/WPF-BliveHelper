using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BliveHelper.Utils
{
    public class Config : ObservableObject
    {
        [JsonIgnore]
        private bool Loaded { get; set; }

        private bool pluginEnabled;
        public bool PluginEnabled
        {
            get => pluginEnabled;
            set => SetProperty(ref pluginEnabled, value);
        }

        private Dictionary<string, string> cookies = new Dictionary<string, string>();
        public Dictionary<string, string> Cookies
        {
            get => cookies;
            set => SetProperty(ref cookies, value);
        }

        private WebSocketSetting webSocket = new WebSocketSetting();
        public WebSocketSetting WebSocket
        {
            get => webSocket;
            set => SetProperty(ref webSocket, value);
        }

        public Config()
        {
            // 如果参数属性发生变动
            PropertyChanged += OnPropertyChanged;
            WebSocket.PropertyChanged += OnPropertyChanged;
        }

        protected async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Loaded && !string.IsNullOrEmpty(ENV.ConfigFileName))
            {
                await SaveAsync();
            }
        }

        public async Task LoadAsync()
        {
            if (File.Exists(ENV.ConfigFileName))
            {
                using (var fs = new FileStream(ENV.ConfigFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                {
                    var configString = await reader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<Config>(configString);
                    if (config != null)
                    {
                        PluginEnabled = config.PluginEnabled;
                        Cookies = config.Cookies;
                        WebSocket.ServerUrl = config.webSocket.ServerUrl;
                        WebSocket.ServerKey = config.webSocket.ServerKey;
                    }
                }
            }
            Loaded = true;
        }

        public async Task SaveAsync()
        {
            using (var fs = new FileStream(ENV.ConfigFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                fs.SetLength(0);
                await sw.WriteAsync(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }
    }
}
