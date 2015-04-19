using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            // XXX: initialize server here
            SdmCore.Destroy();
            return 0;
        }
    }
}
