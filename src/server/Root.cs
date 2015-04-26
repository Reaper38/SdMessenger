using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
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

        private static int SendCommand(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                mutex.ReleaseMutex();
                Console.WriteLine("error: server is not running");
                return -1;
            }
            var command = args[0];
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

        private static void OnConsoleCommand(string command, PipeConsole console)
        {
            // XXX: add users/change settings/etc
            console.WriteLine("not supported yet");
        }
        
        private static int RunServer(string[] args)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("Already running");
                return -1;
            }
            SdmCore.Initialize(AppType.Server);
            using (var pcs = new PipeConsoleServer(OnConsoleCommand))
            {
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
            }
            SdmCore.Destroy();
            mutex.ReleaseMutex();
            return 0;
        }

        private static int PrintUsage(string[] args)
        {
            Console.WriteLine("Usage: <to be documented>");
            return 0;
        }

        private static int Main(string[] args)
        {
            if (args.Length == 0)
                return PrintUsage(args);
            switch (args[0])
            {
            case "run": return RunServer(args);
            case "help": return PrintUsage(args);
            default: return SendCommand(args);
            }
        }
    }
}
