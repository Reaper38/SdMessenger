using System;
using System.IO;
using System.Text;
using Sdm.Core.Util;

namespace Sdm.Core
{
    public class ClientLogger : ILogger
    {
        #region Events

        public event Action<string> MessageLogged;
        public event Action LogCleared;

        private void OnMessageLogged(string msg)
        {
            if (MessageLogged != null)
                MessageLogged(msg);
        }

        private void OnLogCleaned()
        {
            if (LogCleared != null)
                LogCleared();
        }

        #endregion

        private object sync = 0;
        private MemoryStream ms;
        private StreamWriter msw;
        private DateTime startTime;
        private volatile int lineCount;
        private string fDir, fBaseName, fExt;
        private static readonly string dateTimeFormat = "dd.MM.yy-HH:mm:ss";
        private LogLevel minLogLevel;
        private bool disposed = false;

        public ClientLogger(string dir, string baseName, string ext, LogLevel minLogLevel)
        {
            fDir = dir;
            fBaseName = baseName;
            fExt = ext;
            this.minLogLevel = minLogLevel;
            ms = new MemoryStream();
            msw = new StreamWriter(ms, Encoding.Default);
            msw.AutoFlush = true;
            startTime = DateTime.Now;
        }

        public int LineCount
        {
            get { return lineCount; }
            private set { lineCount = value; }
        }

        public void Log(LogLevel logLevel, string message)
        {
            if (logLevel < minLogLevel)
                return;
            lock (sync)
            {
                msw.WriteLine(String.Format("{0} [{1}] {2}",
                    DateTime.Now.ToString(dateTimeFormat), logLevel, message));
                LineCount++;
            }
            OnMessageLogged(message);
        }

        public void Clear()
        {
            lock (sync)
            {
                ms.SetLength(0);
                LineCount = 0;
            }
            OnLogCleaned();
        }

        public void Flush()
        {
            var startTimeStr = startTime.ToString(dateTimeFormat);
            var currentTimeStr = DateTime.Now.ToString(dateTimeFormat);
            var fName = String.Format("{0}/{1}_{2}_{3}{4}", fDir, fBaseName, startTimeStr, currentTimeStr, fExt);
            using (var fs = new FileStream(fName, FileMode.Create))
            {
                ms.WriteTo(fs);
                fs.Flush();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    msw.Close();
                DisposeHelper.OnDispose<ClientLogger>(disposing);
                disposed = true;
            }
        }

        ~ClientLogger() { Dispose(false); }
    }
}
