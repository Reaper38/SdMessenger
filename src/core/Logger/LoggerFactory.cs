using System;

namespace Sdm.Core
{
    public enum LoggerType
    {
        Client,
        Server,
    }

    public sealed class LoggerFactory
    {
        #region Singleton implementation

        private LoggerFactory() {}
        private static readonly LoggerFactory instance = new LoggerFactory();
        public static LoggerFactory Instance { get { return instance; } }

        #endregion

        public ILogger CreateLogger(LoggerType type, LogLevel lvl)
        {
            switch (type)
            {
            case LoggerType.Client: return new ClientLogger(".", "sdm_client", ".log", lvl);
            case LoggerType.Server: return new ServerLogger("sdm_server.log", lvl);
            default: throw new NotSupportedException(type + " is not supported.");
            }
        }
    }
}
