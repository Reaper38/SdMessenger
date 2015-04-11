using System;
using System.IO;
using System.Text;
using Sdm.Core.Util;

namespace Sdm.Core
{
    public class ServerLogger : SdmLoggerBase
    {
        private readonly object sync = 0;
        private FileStream fs;
        private StreamWriter fsw;
        private volatile int lineCount;
        private string logFileName;
        private bool disposed = false;

        public ServerLogger(string logFileName, LogLevel minLogLevel) :
            base(minLogLevel)
        {
            this.logFileName = logFileName;
            fs = new FileStream(logFileName, FileMode.Append);
            fsw = new StreamWriter(fs, Encoding.Default);
            fsw.AutoFlush = true;
        }

        public override int LineCount { get { return lineCount; } }

        public override void Log(LogLevel logLevel, string message)
        {
            if (logLevel < MinLogLevel)
                return;
            lock (sync)
            {
                fsw.WriteLine("{0} [{1}] {2}", DateTime.Now.ToString(DateTimeFormat), logLevel, message);
                lineCount++;
            }
            OnMessageLogged(message);
        }

        public override void Clear() { /* not supported in ServerLogger */ }

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
