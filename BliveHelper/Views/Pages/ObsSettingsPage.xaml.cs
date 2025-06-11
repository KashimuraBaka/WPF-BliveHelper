using BliveHelper.Utils;
using BliveHelper.Utils.Structs;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// ObsSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ObsSettingsPage : UserControl, INotifyPropertyChanged
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
        private bool saveEnable = true;
        public bool SaveEnable
        {
            get => saveEnable;
            set => SetProperty(ref saveEnable, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SaveWebsocketSettingCommand => new RelayCommand(SaveWebsocketSetting);

        public ObsSettingsPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ObsSettingsPage_Loaded;
        }

        private void ObsSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取设定绑定值
            ServerUrl = ENV.Config.WebSocket.ServerUrl;
            ServerKey = ENV.Config.WebSocket.ServerKey;
        }

        private async void SaveWebsocketSetting()
        {
            SaveEnable = false;
            {
                // 保存设置
                ENV.Config.WebSocket.ServerUrl = ServerUrl;
                ENV.Config.WebSocket.ServerKey = ServerKey;
                // 重连 WebSocket 服务
                ENV.WebSocket.Connect(ServerUrl, ServerKey);
                // 保存设置
                await ENV.Config.SaveAsync();
            }
            SaveEnable = true;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}
