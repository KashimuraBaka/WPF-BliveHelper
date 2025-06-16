using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Structs;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// LiveAdminsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LiveAdminsPage : ObservableUserControl
    {
        private int maxAdminsCount;
        public int MaxAdminsCount
        {
            get => maxAdminsCount;
            set => SetProperty(ref maxAdminsCount, value);
        }
        private BliveAdminInfo selectedAdmin;
        public BliveAdminInfo SelectedAdmin
        {
            get => selectedAdmin;
            set => SetProperty(ref selectedAdmin, value);
        }
        private bool addAdminEnabled = true;
        public bool AddAdminEnabled
        {
            get => addAdminEnabled;
            set => SetProperty(ref addAdminEnabled, value);
        }
        private string addAdminContent;
        public string AddAdminContent
        {
            get => addAdminContent;
            set => SetProperty(ref addAdminContent, value);
        }

        public ObservableCollection<BliveAdminInfo> Admins { get; } = new ObservableCollection<BliveAdminInfo>();

        public ICommand AddAdminCommand => new RelayCommand(AddAdmin);
        public ICommand RemoveAdminCommand => new RelayCommand(RemoveAdmin);

        public LiveAdminsPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += LiveAdminsPage_Loaded; ;
        }

        private async void LiveAdminsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var res = await ENV.BliveAPI.GetLiveAdmins(ENV.BliveInfo.RoomId);
            MaxAdminsCount = res.MaxRoomAnchorsNumber;
            Admins.Clear();
            Admins.AddRange(res.Admins);
        }

        private async void RemoveAdmin()
        {
            if (SelectedAdmin != null)
            {
                var result = MessageBox.Show($"确定要移除管理员 {SelectedAdmin.UserName} 吗？", "确认移除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result is MessageBoxResult.Yes)
                {
                    var removeResult = await ENV.BliveAPI.RemoveLiveAdmin(SelectedAdmin.UserId);
                    if (removeResult)
                    {
                        Admins.Remove(SelectedAdmin);
                        SelectedAdmin = null;
                    }
                    else
                    {
                        MessageBox.Show("移除管理员失败，请稍后再试。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void AddAdmin()
        {
            AddAdminEnabled = false;
            if (!string.IsNullOrEmpty(AddAdminContent))
            {
                var result = await ENV.BliveAPI.AddLiveAdmin(AddAdminContent);
                if (result.Success)
                {
                    var newAdmin = new BliveAdminInfo
                    {
                        UserId = result.UserId,
                        UserName = result.UserName,
                        CreationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    Admins.Add(newAdmin);
                    AddAdminContent = string.Empty;
                }
            }
            AddAdminEnabled = true;
        }
    }
}
