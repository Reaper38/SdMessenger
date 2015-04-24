using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Sdm.Core;

namespace Sdm.Server
{
    internal static class Root
    {
        public static void Log(LogLevel lvl, string msg) { SdmCore.Logger.Log(lvl, msg); }

        public static void Log(LogLevel lvl, string format, params object[] args)
        { SdmCore.Logger.Log(lvl, String.Format(format, args)); }

        private static int Main(string[] args)
        {
            SdmCore.Initialize(AppType.Server);
            var cfg = new ServerConfig();
            using (var srv = new Server(cfg))
            {
                srv.Connect(cfg.Address, cfg.Port);
                while (srv.Connected)
                {
                    Thread.Sleep(cfg.UpdateSleep);
                    srv.Update();
                }
            }
            SdmCore.Destroy();
            return 0;
        }
    }
}
