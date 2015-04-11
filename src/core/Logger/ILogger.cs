using System;

namespace Sdm.Core
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public interface ILogger : IDisposable
    {
        event Action<string> MessageLogged;
        event Action LogCleared;
        int LineCount { get; }
        void Log(LogLevel logLevel, string message);
        void Clear();
        void Flush();
    }
}
