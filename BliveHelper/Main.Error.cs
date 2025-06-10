using BliveHelper.Utils;
using System;

namespace BliveHelper
{
    public partial class Main
    {
        private void OnError(object sender, UnhandledExceptionEventArgs e)
        {
            var obj = (Exception)e.ExceptionObject;
            ENV.Log(obj.ToString());
        }
    }
}
