using System;
using System.Threading;
using Sdm.Core;

namespace Sdm.Server
{
    internal static class Root
    {
        private static readonly Mutex mutex = new Mutex(false, "sdm_server_mutex");

        public static void Log(LogLevel lvl, string msg) { SdmCore.Logger.Log(lvl, msg); }

        public static void Log(LogLevel lvl, string format, params object[] args)
        { SdmCore.Logger.Log(lvl, String.Format(format, args)); }
        
        private static int Main(string[] args)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("Already running");
                return 0;
            }
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
            mutex.ReleaseMutex();
            return 0;
        }
    }
}
