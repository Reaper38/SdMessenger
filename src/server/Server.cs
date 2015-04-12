using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Sdm.Core;

namespace Sdm.Server
{
    internal class Server : IPureServer
    {
        #region IPureServer Members

        public IPAddress Address
        {
            get { throw new NotImplementedException(); }
        }

        public int Port
        {
            get { throw new NotImplementedException(); }
        }

        public INetStatistics Stats
        {
            get { throw new NotImplementedException(); }
        }

        public IList<IClient> Clients
        {
            get { throw new NotImplementedException(); }
        }

        public IClient ServerClient
        {
            get { throw new NotImplementedException(); }
        }

        public void Connect(IPAddress address, ushort port)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public IClient CreateClient()
        {
            throw new NotImplementedException();
        }

        public void DestroyClient(IClient cl)
        {
            throw new NotImplementedException();
        }

        public void DisconnectClient(IClient cl, string reason)
        {
            throw new NotImplementedException();
        }

        public void OnMessage(IMessage msg, ClientId cl)
        {
            throw new NotImplementedException();
        }

        public IClient IdToClient(ClientId id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
