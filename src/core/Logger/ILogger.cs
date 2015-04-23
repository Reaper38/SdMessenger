using System;

namespace Sdm.Core
{
    public enum LogLevel
    {
        Verbose = Trace,
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5,
        Off = 6
    }

    public interface ILogger : IDisposable
    {
        event Action<string> MessageLogged;
        event Action LogCleared;
        int LineCount { get; }
        void Log(LogLevel logLevel, string message);
        void Log(LogLevel logLevel, string format, params object[] args);
        void Clear();
        void Flush();
    }
}
