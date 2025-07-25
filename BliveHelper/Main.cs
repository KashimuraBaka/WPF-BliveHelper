﻿using BilibiliDM_PluginFramework;
using BliveHelper.Utils;
using BliveHelper.Views.Windows;

namespace BliveHelper
{
    public partial class Main : DMPlugin
    {
        public MainWindow AdminWindow { get; } = new MainWindow();

        public Main()
        {
            ENV.Plugin = this;

            PluginAuth = "Kashimura";
            PluginName = "BliveHelper";
            PluginCont = "kashimura@qq.com";
            PluginVer = ENV.AppVersion;
            PluginDesc = "Bilibili直播 开播助手";

            Connected += OnConnected;
            Disconnected += OnDisconnected;
            ReceivedDanmaku += OnReceivedDanmaku;
            ReceivedRoomCount += OnReceivedRoomCount;
        }
    }
}
