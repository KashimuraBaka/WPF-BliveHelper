using BilibiliDM_PluginFramework;
using BliveHelper.Utils;
using System;

namespace BliveHelper
{
    public partial class Main : DMPlugin
    {
        public Main()
        {
            ENV.DanMuPlugin = this;

            PluginAuth = "Kashimura";
            PluginName = "BliveHelper";
            PluginCont = "kashimura@qq.com";
            PluginVer = GetVersion();
            PluginDesc = "Bilibili直播 开播助手";

            Connected += DanMuJi_Connected;
            Disconnected += DanMuJi_Disconnected;
            AppDomain.CurrentDomain.UnhandledException += OnError;
        }

        private void DanMuJi_Connected(object sender, ConnectedEvtArgs e)
        {
        }

        private void DanMuJi_Disconnected(object sender, DisconnectEvtArgs e)
        {
        }
    }
}
