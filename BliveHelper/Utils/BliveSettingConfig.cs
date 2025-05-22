using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;

namespace BliveHelper.Utils
{
    public partial class BliveSettingConfig : ObservableObject
    {
        private string FileName { get; set; } = string.Empty;

        [ObservableProperty]
        [property: System.Text.Json.Serialization.JsonPropertyName("cookies")]
        private Dictionary<string, string> cookies = [];

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
                }
            }
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                using var fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write);
                fs.SetLength(0);
                using var sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(JsonSerializer.Serialize(this));
            }
        }
    }
}
