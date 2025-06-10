using BliveHelper.Utils;
using BliveHelper.Views.Windows;
using System.IO;
using System.Threading;

namespace BliveHelper
{
    public partial class Main
    {
        public override async void Inited()
        {
            ENV.Log("加载配置中...");
            if (!Directory.Exists(ENV.ConfigDirectory))
            {
                Log("未发现配置文件夹，尝试创建中");
                Directory.CreateDirectory(ENV.ConfigDirectory);
            }
            await ENV.Config.LoadAsync();
            ENV.Log("配置加载完毕!");
        }

        public override void Admin()
        {
            var windowThread = new Thread(() =>
            {
                new MainWindow().ShowDialog();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }
    }
}
