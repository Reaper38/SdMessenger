using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;

namespace Sdm.Core
{
    // represents local client (for client app)
    public abstract class PureClientBase : IDisposable
    {
        public event Action<ConnectionResult> ConnectionResult;

        public abstract ConnectionState ConnectionState { get; }
        public abstract ClientId Id { get; }
        public abstract IPAddress ServerAddress { get; }
        public abstract ushort ServerPort { get; }
        public abstract INetStatistics Stats { get; }
        // connection result have to be checked using ConnectionResult event
        public abstract void Connect(IPAddress address, ushort port, string login, string password);
        public abstract void Disconnect();
        public abstract void OnMessage(IMessage msg);

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        ~PureClientBase() { Dispose(false); }

        #endregion
    }
}
