using System;
using System.Windows.Forms;
using Sdm.Core;

namespace Sdm.Client
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(AppController.Instance);
            AppController.Instance.Config.Save();
            SdmCore.Destroy();
        }
    }
}
