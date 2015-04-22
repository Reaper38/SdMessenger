using System;
using System.Net;

namespace Sdm.Core
{
    // represents local client (for client app)
    public abstract class PureClientBase : IDisposable
    {
        public event Action<ConnectionResult, string> ConnectionResult;
        public event Action<AuthResult, string> AuthResult;

        public abstract ConnectionState ConnectionState { get; }
        public abstract ClientId Id { get; }
        public abstract IPAddress ServerAddress { get; }
        public abstract ushort ServerPort { get; }
        public abstract INetStatistics Stats { get; }
        // connection result have to be checked using ConnectionResult event
        public abstract void Connect(IPAddress address, ushort port, string login, string password);
        public abstract void Disconnect();
        public abstract void OnMessage(IMessage msg);
        public abstract void Send(IMessage msg);

        protected void OnConnectionResult(ConnectionResult cr, string msg = null)
        {
            if (ConnectionResult != null)
                ConnectionResult(cr, msg);
        }

        protected void OnAuthResult(AuthResult ar, string msg = null)
        {
            if (AuthResult != null)
                AuthResult(ar, msg);
        }

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
