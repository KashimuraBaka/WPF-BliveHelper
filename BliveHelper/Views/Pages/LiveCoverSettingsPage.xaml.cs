using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Structs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// LiveCoverSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LiveCoverSettingsPage : ObservableUserControl
    {
        public ObservableCollection<BliveCoverInfo> Covers { get; } = new ObservableCollection<BliveCoverInfo>();
        // 选择背景
        private BliveCoverInfo selectedCover;
        public BliveCoverInfo SelectedCover
        {
            get => selectedCover;
            set
            {
                OnSelectedCoverChanged(selectedCover, value);
                SetProperty(ref selectedCover, value);
            }
        }

        public LiveCoverSettingsPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += LiveCoverSettingsPage_Loaded;
        }

        private async void LiveCoverSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Covers.Clear();
            Covers.AddRange(await ENV.BliveAPI.GetLiveCovers());
            SelectedCover = Covers.FirstOrDefault(x => x.SelectStatus);
        }

        private void OnSelectedCoverChanged(BliveCoverInfo oldCover, BliveCoverInfo newCover)
        {
            if (oldCover == null || newCover == null) return;

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                var result = MessageBox.Show("是否更换封面?", "更换直播封面", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result is MessageBoxResult.OK)
                {
                    var message = await ENV.BliveAPI.ReplaceLiveCover(ENV.BliveInfo.RoomId, newCover.ID, newCover.Url);
                    if (string.IsNullOrEmpty(message)) return;
                    MessageBox.Show(message, "更换封面错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                selectedCover = oldCover; // 恢复旧封面
                NotifyPropertyChanged(nameof(SelectedCover));
            }));
        }
    }
}
