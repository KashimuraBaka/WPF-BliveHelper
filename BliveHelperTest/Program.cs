using BliveHelper;
using System;
using System.Threading.Tasks;

namespace BliveHelperTest
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] _)
        {
            var main = new Main();
            main.Inited();
            Task.Delay(1000).Wait();
            main.Debugindow();
        }
    }
}
