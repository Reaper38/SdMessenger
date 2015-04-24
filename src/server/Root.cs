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
            using (var srv = new Server())
            {
                srv.Connect(IPAddress.Any, 5477); // load from config
                while (srv.Connected)
                {
                    Thread.Sleep(50); // XXX: load from config
                    srv.Update();
                }
            }
            SdmCore.Destroy();
            return 0;
        }
    }
}
