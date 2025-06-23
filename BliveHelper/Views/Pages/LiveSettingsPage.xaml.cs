using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Structs;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// LiveSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LiveSettingsPage : ObservableUserControl
    {
        public BliveInfo Blive => ENV.BliveInfo;

        private bool startEnable = true;
        public bool StartEnable
        {
            get => startEnable;
            set => SetProperty(ref startEnable, value);
        }

        public string ActionButtonText => ENV.Config.WebSocket.AutoStream ? "已开启自动开播" : (Blive.IsStart ? "停止直播" : "开始直播");
        public ICommand ChangedSettingCommand => new RelayCommand(async () => MessageBox.Show(await Blive.SaveSetting()));
        public ICommand ActionLiveCommand => new RelayCommand(ActionLive);
        public ICommand SetBliveOpCodeCommand => new RelayCommand(SetBliveOpCode);

        public LiveSettingsPage() : base()
        {
            InitializeComponent();
            // 绑定事件
            Blive.PropertyChanged += Blive_OnPropertyChanged;
            Loaded += LiveSettingsPage_Loaded;
        }

        private void LiveSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            StartEnable = !ENV.Config.WebSocket.AutoStream;
            NotifyPropertyChanged(nameof(ActionButtonText));
        }

        private void Blive_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Blive.IsStart):
                    NotifyPropertyChanged(nameof(ActionButtonText));
                    break;
            }
        }

        private async void ActionLive()
        {
            StartEnable = false;
            var result = await Blive.ToggleStreamLive();
            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show(result);
            }
            StartEnable = true;
        }

        private void SetBliveOpCode()
        {
            var window = ENV.AppWindow;
            if (window != null && window.FindName("OPCode") is PasswordBox opCodeTextBox)
            {
                opCodeTextBox.Password = Blive.BroadcastCode;
                MessageBox.Show("设置身份码完成!");
            }
        }
    }
}
