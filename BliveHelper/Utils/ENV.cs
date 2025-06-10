using System.IO;
using System.Reflection;

namespace BliveHelper.Utils
{
    public static class ENV
    {
        public static Main DanMuPlugin { get; set; }
        public static Config Config { get; } = new Config();

        public static string AppDllFileName { get; } = Assembly.GetExecutingAssembly().Location;
        public static string AppDllFilePath { get; } = new FileInfo(AppDllFileName).DirectoryName;
        public static string ConfigDirectory { get; } = Path.Combine(AppDllFilePath, "BliveHelper");
        public static string ConfigFileName { get; } = Path.Combine(ConfigDirectory, "config.json");

        public static void Log(string message) => DanMuPlugin?.Log(message);
    }
}
