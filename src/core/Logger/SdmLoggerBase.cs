using System;

namespace Sdm.Core
{
    internal abstract class SdmLoggerBase : ILogger
    {
        public event Action<LogLevel, DateTime, string> MessageLogged;

        protected void OnMessageLogged(LogLevel lvl, DateTime time, string msg)
        {
            if (MessageLogged != null)
                MessageLogged(lvl, time, msg);
        }

        protected static string FormatLogLevel(LogLevel l)
        { return l.ToString().ToLower(); }

        protected static readonly string DateTimeFormat = "dd.MM.yy-HH:mm:ss";
        protected LogLevel MinLogLevel;

        protected SdmLoggerBase(LogLevel minLogLevel)
        { MinLogLevel = minLogLevel; }

        public abstract void Log(LogLevel logLevel, string message);

        public void Log(LogLevel logLevel, string format, params object[] args)
        { Log(logLevel, String.Format(format, args)); }

        public abstract void Flush();
        public abstract void Dispose();
    }
}
