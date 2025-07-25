﻿using BilibiliDM_PluginFramework;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Obs;
using BliveHelper.Utils.Structs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace BliveHelper.Utils
{
    public static class ENV
    {
        public static Main Plugin { get; set; }
        public static Config Config { get; } = new Config();
        public static BliveAPI BliveAPI { get; } = new BliveAPI();
        public static BliveInfo BliveInfo { get; } = new BliveInfo();
        public static ObservableCollection<Danmaku> Danmakus { get; } = new ObservableCollection<Danmaku>();

        public static Window AppWindow => Application.Current?.MainWindow;
        public static Assembly AppAssembly { get; } = Assembly.GetExecutingAssembly();
        public static string AppVersion => AppAssembly.GetName().Version.ToString();
        public static string AppDllFileName => AppAssembly.Location;
        public static string AppDllFilePath { get; } = new FileInfo(AppDllFileName).DirectoryName;
        public static string ConfigDirectory { get; } = Path.Combine(AppDllFilePath, "BliveHelper");
        public static string ConfigFileName { get; } = Path.Combine(ConfigDirectory, "config.json");

        public static ObsWebSocketAPI WebSocket { get; } = new ObsWebSocketAPI();

        public static async Task InitServices()
        {
            await Config.LoadAsync();
            // 设置 Cookies
            BliveAPI.Cookies = Config.Cookies;
            // 尝试启动 WebSocket 服务
            WebSocket.Connect(Config.WebSocket.ServerUrl, Config.WebSocket.ServerKey);
        }

        public static async void AddDanmaku(DanmakuModel danmakuRawData)
        {
            Danmaku danmaku;
            if (danmakuRawData.RawDataJToken != null)
            {
                danmaku = danmakuRawData.RawDataJToken.ToObject<DanmakuRawData>().Data;
                // 如果之前记录已经获取到 UID, 则直接覆盖
                var hasUIDUser = Danmakus.FirstOrDefault(x => x.UserId != 0 && x.UserName == danmaku.UserName);
                if (hasUIDUser != null)
                {
                    danmaku.UserId = hasUIDUser.UserId;
                }
                else
                {
                    var uids = await BliveAPI.NameToUID(new string[] { danmaku.UserName });
                    danmaku.UserId = uids.FirstOrDefault()?.UID ?? 0;
                }
            }
            else
            {
                danmaku = new Danmaku()
                {
                    MessageTime = DateTimeOffset.Now.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = danmakuRawData.UserName,
                    Message = danmakuRawData.CommentText,
                };
            }
            // 记录当前弹幕
            Danmakus.Insert(0, danmaku);
            if (Danmakus.Count > 100)
            {
                Danmakus.RemoveAt(Danmakus.Count - 1);
            }
        }

        public static void Log(string message) => Plugin?.Log(message);
    }
}
