using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Structs;
using System.ComponentModel;
using System.Windows;
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

        public string ActionButtonText => ENV.Config.WebSocket.AutoStream ? $"已开启自动开播 ({AutoStreamStatusText})" : ManualStreamStatusText;
        public string ManualStreamStatusText => Blive.IsStart ? "停止直播" : "开始直播";
        public string AutoStreamStatusText => Blive.IsStart ? "正在直播" : "未开播";
        public ICommand ChangedSettingCommand => new RelayCommand(async () => MessageBox.Show(await Blive.SaveSetting()));
        public ICommand ActionLiveCommand => new RelayCommand(ActionLive);

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
    }
}
