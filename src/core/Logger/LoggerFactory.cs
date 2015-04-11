using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdm.Core
{
    public sealed class LoggerFactory
    {
        #region Singleton implementation

        private LoggerFactory() {}
        private static readonly LoggerFactory instance = new LoggerFactory();
        public static LoggerFactory Instance { get { return instance; } }

        #endregion

        public ILogger CreateLogger(object user, LogLevel lvl)
        {
            // todo: implement CreateLogger
            /* supposed implementation:
            if (user is IServer)
                return new ServerLogger("sdm_server.log", lvl);
            if (user is IClient)
                return new ClientLogger(".", "sdm_client", ".log", lvl);
            throw new NotSupportedException("There's no logger provided for specified user.");
            */
            throw new NotImplementedException();
        }
    }
}
