using System;
using System.IO;
using System.Text;
using Sdm.Core.Util;

namespace Sdm.Core
{
    internal class ClientLogger : SdmLoggerBase
    {
        private readonly object sync = 0;
        private MemoryStream ms;
        private StreamWriter msw;
        private DateTime startTime;
        private volatile int lineCount;
        private string fDir, fBaseName, fExt;
        private volatile bool flushRequired = false;
        private bool disposed = false;

        public ClientLogger(string dir, string baseName, string ext, LogLevel minLogLevel) :
            base(minLogLevel)
        {
            fDir = dir;
            fBaseName = baseName;
            fExt = ext;
            ms = new MemoryStream();
            msw = new StreamWriter(ms, Encoding.Default);
            msw.AutoFlush = true;
            startTime = DateTime.Now;
        }

        public override int LineCount { get { return lineCount; } }

        public override void Log(LogLevel logLevel, string message)
        {
            if (logLevel < MinLogLevel)
                return;
            lock (sync)
            {
                msw.WriteLine("{0} [{1}] {2}", DateTime.Now.ToString(DateTimeFormat),
                    FormatLogLevel(logLevel), message);
                lineCount++;
                flushRequired = true;
            }
            OnMessageLogged(message);
        }

        public override void Clear()
        {
            lock (sync)
            {
                ms.SetLength(0);
                lineCount = 0;
            }
            OnLogCleaned();
        }

        public override void Flush()
        {
            const string fsDateTimeFormat = "dd.MM.yy-HH.mm.ss";
            lock (sync)
            {
                var startTimeStr = startTime.ToString(fsDateTimeFormat);
                var currentTimeStr = DateTime.Now.ToString(fsDateTimeFormat);
                var fName = String.Format("{0}/{1}_{2}_{3}{4}", fDir, fBaseName, startTimeStr, currentTimeStr, fExt);
                using (var fs = new FileStream(fName, FileMode.Create))
                {
                    ms.WriteTo(fs);
                    fs.Flush();
                }
                flushRequired = false;
            }
        }

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
                {
                    if (flushRequired)
                        Flush();
                    msw.Close();
                }
                DisposeHelper.OnDispose<ClientLogger>(disposing);
                disposed = true;
            }
        }

        ~ClientLogger() { Dispose(false); }
    }
}
