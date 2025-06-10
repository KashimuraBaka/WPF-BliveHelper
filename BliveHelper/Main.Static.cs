using System.Reflection;

namespace BliveHelper
{
    public partial class Main
    {
        private static string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
