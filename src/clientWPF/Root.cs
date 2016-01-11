using Sdm.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sdm.ClientWPF
{
    internal static class Root
    {
        public static void Log(LogLevel lvl, string msg) { SdmCore.Logger.Log(lvl, msg); }

        public static void Log(LogLevel lvl, string format, params object[] args)
        { SdmCore.Logger.Log(lvl, String.Format(format, args)); }

        [STAThread]
        private static void Main()
        {
            SdmCore.Initialize(AppType.Client);
            var appc = AppController.Instance;

            AppController.Instance.Config.Save();
            SdmCore.Destroy();
        }
    }
}
