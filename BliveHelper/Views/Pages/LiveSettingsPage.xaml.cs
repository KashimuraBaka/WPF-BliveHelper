﻿using BliveHelper.Utils;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BliveHelper.Views.Pages
{
    /// <summary>
    /// LiveSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LiveSettingsPage : UserControl, INotifyPropertyChanged
    {
        public BliveInfo Info => ENV.BliveInfo;

        private bool startEnable = true;
        public bool StartEnable
        {
            get => startEnable;
            set => SetProperty(ref startEnable, value);
        }

        public string ActionButtonText => Info.IsStart ? "停止直播" : "开始直播";
        public ICommand ChangedSettingCommand => new RelayCommand(ChangedSetting);
        public ICommand ActionLiveCommand => new RelayCommand(ActionLive);
        public int GameAreaID => BaseLiveAreas.FirstOrDefault(x => x.Name == Info.SelectedArea)?.List.FirstOrDefault(x => x.Name == Info.SelectedGame)?.Id ?? 0;
        private List<BliveArea> BaseLiveAreas { get; } = new List<BliveArea>();
        public ObservableCollection<string> LiveAreas { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> LiveGames { get; } = new ObservableCollection<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        public LiveSettingsPage()
        {
            InitializeComponent();
            DataContext = this;
            Info.PropertyChanged += Info_PropertyChanged;
            ReadLiveAreas();
        }

        private async void ReadLiveAreas()
        {
            // 添加直播分区
            BaseLiveAreas.Clear();
            BaseLiveAreas.AddRange(await ENV.BliveAPI.GetAreas());
            LiveAreas.Clear();
            foreach (var area in BaseLiveAreas)
            {
                LiveAreas.Add(area.Name);
            }
        }

        private void Info_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Info.IsStart):
                    NotifyPropertyChanged(nameof(ActionButtonText));
                    break;
                case nameof(Info.SelectedArea):
                    // 切换分区时, 刷新游戏列表
                    if (!string.IsNullOrEmpty(Info.SelectedArea))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LiveGames.Clear();
                            var area = BaseLiveAreas.FirstOrDefault(x => x.Name == Info.SelectedArea);
                            if (area != null)
                            {
                                foreach (var game in area.List)
                                {
                                    LiveGames.Add(game.Name);
                                }
                            }
                        });
                    }
                    break;
            }
        }

        private async void ChangedSetting()
        {
            if (!string.IsNullOrEmpty(Info.SelectedArea) && !string.IsNullOrEmpty(Info.SelectedGame))
            {
                var result = await ENV.BliveAPI.SetLiveInfo(Info.RoomId, Info.Title, GameAreaID);
                if (result.Success)
                {
                    MessageBox.Show("修改完毕!");
                }
                else
                {
                    MessageBox.Show(result.Message);
                }
            }
            else
            {
                MessageBox.Show("请选择直播分区");
            }
        }

        private async void ActionLive()
        {
            StartEnable = false;
            if (!Info.IsStart)
            {
                if (!string.IsNullOrEmpty(Info.SelectedArea) && !string.IsNullOrEmpty(Info.SelectedGame))
                {
                    var rtmp = await ENV.BliveAPI.StartLive(Info.RoomId, Info.Title, GameAreaID);
                    if (rtmp != null && !string.IsNullOrEmpty(rtmp.ServerUrl))
                    {
                        Info.IsStart = true;
                        Info.StreamServerUrl = rtmp.ServerUrl;
                        Info.StreamServerKey = rtmp.Code;
                        await ENV.WebSocket.SetStreamServiceSettings(rtmp.ServerUrl, rtmp.Code);
                        await ENV.WebSocket.StartStream();
                    }
                    else
                    {
                        MessageBox.Show("获取推流地址失败, 可能网络问题或者当前分区不支持推流码获取");
                    }
                }
                else
                {
                    MessageBox.Show("请选择直播分区");
                }
            }
            else
            {
                var res = await ENV.BliveAPI.StopLive(Info.RoomId);
                if (res != null && res.Change == 1)
                {
                    Info.IsStart = false;
                    await ENV.WebSocket.StopStream();
                }
            }
            StartEnable = true;
        }

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
    }
}
