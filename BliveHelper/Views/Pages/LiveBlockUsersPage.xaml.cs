using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// LiveBlockUsersPage.xaml 的交互逻辑
    /// </summary>
    public partial class LiveBlockUsersPage : UserControl, INotifyPropertyChanged
    {
        private Danmaku selectedDanmaku;
        public Danmaku SelectedDanmaku
        {
            get => selectedDanmaku;
            set => SetProperty(ref selectedDanmaku, value);
        }
        private BliveBlockUserInfo selectedBlockUser;
        public BliveBlockUserInfo SelectedBlockUser
        {
            get => selectedBlockUser;
            set => SetProperty(ref selectedBlockUser, value);
        }

        public ObservableCollection<Danmaku> Danmakus => ENV.Danmakus;
        public ObservableCollection<BliveBlockUserInfo> BlockUsers { get; } = new ObservableCollection<BliveBlockUserInfo>();

        public ICommand CopyUIDCommand => new RelayCommand(SaveUID);
        public ICommand BlockUserCommand => new RelayCommand<string>(BlockUser);
        public ICommand RemoveBlockUserCommand => new RelayCommand(RemoveBlockUser);

        public event PropertyChangedEventHandler PropertyChanged;

        public LiveBlockUsersPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += LiveBlockUsersPage_Loaded;
        }

        private void LiveBlockUsersPage_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshBlockUsers();
        }

        public void SaveUID()
        {
            Clipboard.SetDataObject(SelectedDanmaku.UserId.ToString());
        }

        private async void RefreshBlockUsers()
        {
            BlockUsers.Clear();
            BlockUsers.AddRange(await ENV.BliveAPI.GetBlockUsers(ENV.BliveInfo.RoomId));
        }

        private async void BlockUser(string timeStr)
        {
            if (int.TryParse(timeStr, out var time))
            {
                var message = await ENV.BliveAPI.BlockUser(ENV.BliveInfo.RoomId, SelectedDanmaku.UserId.ToString(), time);
                if (!string.IsNullOrEmpty(message))
                {
                    MessageBox.Show(message, "封禁用户失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    RefreshBlockUsers();
                }
            }
        }

        private async void RemoveBlockUser()
        {
            if (SelectedBlockUser == null) return;
            var message = await ENV.BliveAPI.RemoveBlockUser(ENV.BliveInfo.RoomId, SelectedBlockUser.BlockId);
            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message, "解除封禁失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                BlockUsers.Remove(SelectedBlockUser);
            }
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
