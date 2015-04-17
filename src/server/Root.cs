using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdm.Core;

namespace Sdm.Server
{
    internal static class Root
    {
        private static ILogger logger;

        public static void Log(LogLevel lvl, string msg) { logger.Log(lvl, msg); }

        public static void Log(LogLevel lvl, string format, params object[] args)
        { logger.Log(lvl, String.Format(format, args)); }

        private static int Main(string[] args)
        {
            using (logger = LoggerFactory.Instance.CreateLogger(LoggerType.Server, LogLevel.Trace))
            {
                // XXX: initialize server here
            }
            return 0;
        }
    }
}
