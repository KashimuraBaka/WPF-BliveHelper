using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// LiveCoverSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LiveCoverSettingsPage : UserControl, INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

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

        #region MVVM Helpers
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                NotifyPropertyChanged(propertyName);
            }
        }
        #endregion
    }
}
