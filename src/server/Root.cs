using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Sdm.Core;
using Sdm.Core.Util;

namespace Sdm.Server
{
    internal static class Root
    {
        public const string AppName = "SdmServer";
        private static readonly Mutex mutex = new Mutex(false, "sdm_server_mutex");
        public static Server Server { get; private set; }
        public static CommandManager CmdMgr { get; private set; }

        public static void Log(LogLevel lvl, string msg) { SdmCore.Logger.Log(lvl, msg); }

        public static void Log(LogLevel lvl, string format, params object[] args)
        { SdmCore.Logger.Log(lvl, String.Format(format, args)); }

        private static int SendCommand(string[] args)
        {
            if (args.Length == 0)
                return 0;
            var sb = new StringBuilder(1024);
            sb.Append(StringUtil.EscapeCmdString(args[0], true));
            for (int i = 1; i < args.Length; i++)
                sb.Append(' ').Append(StringUtil.EscapeCmdString(args[i], true));
            var cmd = sb.ToString();
            return SendCommand(cmd);
        }

        private static int SendCommand(string command)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                mutex.ReleaseMutex();
                Console.WriteLine("error: server is not running");
                return -1;
            }
            string tmpPipeName = null;
            using (var pipe = new NamedPipeClientStream(".", PipeConsoleServer.PipeName, PipeDirection.InOut))
            {
                try
                { pipe.Connect(1000); }
                catch (TimeoutException)
                {
                    Console.WriteLine("error: connection timed out");
                    return -1;
                }
                var reader = new StreamReader(pipe, true);
                tmpPipeName = reader.ReadLine();
            }
            // next, create new pipe to send/receive commands
            using (var clPipe = new NamedPipeClientStream(".", tmpPipeName, PipeDirection.InOut))
            {
                try
                { clPipe.Connect(1000); }
                catch (TimeoutException)
                {
                    Console.WriteLine("error: connection timed out");
                    return -1;
                }
                var clReader = new StreamReader(clPipe, true);
                var clWriter = new StreamWriter(clPipe, Encoding.UTF8);
                // send initial command
                clWriter.WriteLine(command);
                clWriter.Flush();
                // fetch all available data from pipe
                while (true)
                {
                    var reply = clReader.ReadLine();
                    if (reply == null)
                        break;
                    Console.WriteLine(reply);
                }
            }
            return 0;
        }
                
        private static int RunServer(string[] args)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine(AppName + ": already running");
                return -1;
            }
            SdmCore.Initialize(AppType.Server);
            var cfg = new ServerConfig();
            CmdMgr = new CommandManager();
            using (Server = new Server(cfg))
            {
                using (var pcs = new PipeConsoleServer(CmdMgr.HandleCommand))
                {
                    Server.Connect(cfg.Address, cfg.Port);
                    while (Server.Connected)
                    {
                        Thread.Sleep(cfg.UpdateSleep);
                        Server.Update();
                    }
                }
            }
            SdmCore.Destroy();
            mutex.ReleaseMutex();
            return 0;
        }
        
        private static int Main(string[] args)
        {
            if (args.Length == 0)
                args = new[] {"help"};
            switch (args[0])
            {
            case "run": return RunServer(args);
            default: return SendCommand(args);
            }
        }
    }
}
