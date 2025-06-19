using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Structs;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// LiveBackgroundsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LiveBackgroundsPage : ObservableUserControl
    {
        private BliveBackgroundInfo selectedBackground;
        public BliveBackgroundInfo SelectedBackground
        {
            get => selectedBackground;
            set
            {
                OnSelectedCoverChanged(SelectedBackground, value);
                SetProperty(ref selectedBackground, value);
            }
        }

        public ObservableCollection<BliveBackgroundInfo> Backgrounds { get; } = new ObservableCollection<BliveBackgroundInfo>();

        public LiveBackgroundsPage() : base()
        {
            InitializeComponent();
            Loaded += LiveBackgroundsPage_Loaded;
        }

        private async void LiveBackgroundsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var backgrounds = await ENV.BliveAPI.GetLiveBackgrounds(ENV.BliveInfo.RoomId);
            if (backgrounds.Count > 0)
            {
                Backgrounds.Clear();
                Backgrounds.AddRange(backgrounds);
            }
        }

        private void OnSelectedCoverChanged(BliveBackgroundInfo oldBackground, BliveBackgroundInfo newBackground)
        {
            if (newBackground == null) return;

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                var result = MessageBox.Show("是否要更换背景?", "更换直播背景", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result is MessageBoxResult.OK)
                {
                    var success = await ENV.BliveAPI.UpdateLiveBackground(ENV.BliveInfo.RoomId, newBackground.Id);
                    if (success) return;
                    MessageBox.Show("更换背景失败!", "更换直播背景", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                selectedBackground = oldBackground; // 恢复旧封面
                NotifyPropertyChanged(nameof(SelectedBackground));
            }));
        }
    }
}
