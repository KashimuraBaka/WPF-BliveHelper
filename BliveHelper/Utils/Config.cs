using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BliveHelper.Utils
{
    public class Config : ObservableObjectNotify
    {
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
            WebSocket.PropertyChanged += OnPropertyChanged;
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public async Task LoadAsync()
        {
            if (File.Exists(ENV.ConfigFileName))
            {
                using (var reader = new StreamReader(ENV.ConfigFileName, Encoding.UTF8))
                {
                    var configString = await reader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<Config>(configString);
                    if (config != null)
                    {
                        Cookies = config.Cookies;
                        WebSocket = config.webSocket;
                    }
                }
            }
        }

        public async Task SaveAsync()
        {
            using (var fs = new FileStream(ENV.ConfigFileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.SetLength(0);
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    await sw.WriteAsync(JsonConvert.SerializeObject(this, Formatting.Indented));
                }
            }
        }
    }
}
