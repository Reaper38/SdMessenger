using System;
using System.IO;
using System.Text;
using Sdm.Core.Util;

namespace Sdm.Core
{
    internal class ServerLogger : SdmLoggerBase
    {
        private readonly object sync = 0;
        private FileStream fs;
        private StreamWriter fsw;
        private string logFileName;
        private bool disposed = false;

        public ServerLogger(string logFileName, LogLevel minLogLevel) :
            base(minLogLevel)
        {
            this.logFileName = logFileName;
            fs = new FileStream(logFileName, FileMode.Append);
            fsw = new StreamWriter(fs, Encoding.Default);
            fsw.AutoFlush = true;
            var separator = new string('=', 80);
            fsw.WriteLine(separator);
        }
        
        public override void Log(LogLevel logLevel, string message)
        {
            if (logLevel < MinLogLevel)
                return;
            var time = DateTime.Now;
            lock (sync)
            {
                var fstr = String.Format("{0} [{1}] {2}", time.ToString(DateTimeFormat),
                    FormatLogLevel(logLevel), message);
#if LOG_TO_CONSOLE
                Console.WriteLine(fstr);
#endif
                fsw.WriteLine(fstr);
            }
            OnMessageLogged(logLevel, time, message);
        }

        public override void Flush() { /* filestream flushes automatically */ }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    fsw.Close();
                DisposeHelper.OnDispose<ServerLogger>(disposing);
                disposed = true;
            }
        }

        ~ServerLogger() { Dispose(false); }
    }
}
