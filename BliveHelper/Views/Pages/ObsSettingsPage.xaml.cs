using BliveHelper.Utils;
using BliveHelper.Utils.Structs;
using System.Windows;
using System.Windows.Input;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// ObsSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ObsSettingsPage : ObservableUserControl
    {
        private string serverUrl;
        public string ServerUrl
        {
            get => serverUrl;
            set => SetProperty(ref serverUrl, value);
        }
        private string serverKey;
        public string ServerKey
        {
            get => serverKey;
            set => SetProperty(ref serverKey, value);
        }
        private bool autoStream;
        public bool AutoStream
        {
            get => autoStream;
            set => SetProperty(ref autoStream, value);
        }
        private bool saveEnable = true;
        public bool SaveEnable
        {
            get => saveEnable;
            set => SetProperty(ref saveEnable, value);
        }

        public ICommand SaveWebsocketSettingCommand => new RelayCommand(SaveWebsocketSetting);

        public ObsSettingsPage() : base()
        {
            InitializeComponent();
            Loaded += ObsSettingsPage_Loaded;
        }

        private void ObsSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取设定绑定值
            ServerUrl = ENV.Config.WebSocket.ServerUrl;
            ServerKey = ENV.Config.WebSocket.ServerKey;
            AutoStream = ENV.Config.WebSocket.AutoStream;
        }

        private async void SaveWebsocketSetting()
        {
            SaveEnable = false;
            {
                // 保存设置
                ENV.Config.WebSocket.ServerUrl = ServerUrl;
                ENV.Config.WebSocket.ServerKey = ServerKey;
                ENV.Config.WebSocket.AutoStream = AutoStream;
                // 重连 WebSocket 服务
                ENV.WebSocket.Connect(ServerUrl, ServerKey);
                // 保存设置
                await ENV.Config.SaveAsync();
            }
            SaveEnable = true;
        }
    }
}
