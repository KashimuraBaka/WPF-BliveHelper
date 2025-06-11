using BilibiliDM_PluginFramework;
using BliveHelper.Utils.Blive;
using BliveHelper.Utils.Obs;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BliveHelper.Utils
{
    public static class ENV
    {
        public static DMPlugin DanMuPlugin { get; set; }
        public static Config Config { get; } = new Config();
        public static BliveAPI BliveAPI { get; } = new BliveAPI();

        public static string AppDllFileName { get; } = Assembly.GetExecutingAssembly().Location;
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

        public static void Log(string message) => DanMuPlugin?.Log(message);
    }
}
