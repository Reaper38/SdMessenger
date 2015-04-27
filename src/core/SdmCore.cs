using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sdm.Core
{
    public enum AppType
    {
        Client,
        Server,
    }

    public static class SdmCore
    {
        public static ILogger Logger { get; private set; }
        public static IniFile Config { get; private set; }

        private static volatile bool initialized = false;
        private static readonly object sync = 0;

        static SdmCore()
        {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif
        }

        private static string GetConfigFileName(AppType app)
        {
            switch (app)
            {
            case AppType.Client: return "sdm_client.ini";
            case AppType.Server: return "sdm_server.ini";
            default: throw new NotSupportedException(app + " is not supported.");
            }
        }

        public static void Initialize(AppType app)
        {
            lock (sync)
            {
                if (initialized)
                    return;
                var lvl = LogLevel.Info;
                try
                {
                    Config = new IniFile(GetConfigFileName(app), Encoding.UTF8);
                }
                catch (IOException)
                {
                    Config = IniFile.Empty;
                }
                do
                {
                    if (!Config.ContainsSection("log"))
                        break;
                    var levelIndex = -1;
                    if (!Config.TryGetInt32("log", "level", ref levelIndex))
                        break;
                    try
                    {
                        lvl = (LogLevel) levelIndex;
                    }
                    catch (InvalidCastException)
                    {
                    } // in case of cast failure, logLevel will have its initial value
                } while (false);
                switch (app)
                {
                    case AppType.Client:
                        Logger = new ClientLogger(".", "sdm_client", ".log", lvl);
                        break;
                    case AppType.Server:
                        Logger = new ServerLogger("sdm_server.log", lvl);
                        break;
                    default:
                        throw new NotSupportedException(app + " is not supported.");
                }
                var asmVer = Assembly.GetExecutingAssembly().GetName().Version;
#if DEBUG
                const string bconf = " [debug]";
#else
                const string bconf = "";
#endif
                var ver = String.Format("v.{0}.{1}.{2}{3}", asmVer.Major, asmVer.Minor, asmVer.Build, bconf);
                Logger.Log(LogLevel.Info, "SdmCore initialized: " + ver);
                initialized = true;
            }
        }

        public static void Destroy()
        {
            lock (sync)
            {
                if (!initialized)
                    return;
                Logger.Log(LogLevel.Trace, "SdmCore deinitializing");
                Config = null;
                Logger.Dispose();
                Logger = null;
                initialized = false;
            }
        }

#if !DEBUG
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = (Exception)e.ExceptionObject;
                if (SdmCore.Logger != null)
                    SdmCore.Logger.Log(LogLevel.Fatal, ex.ToString());
                SdmCore.Destroy();
            }
            catch
            { }
            Environment.FailFast("Unhandled exception", ex);
        }
#endif
    }
}
