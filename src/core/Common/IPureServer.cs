using System;
using System.Collections.Generic;
using System.Net;

namespace Sdm.Core
{
    // represents server by itself (for server app)
    public interface IPureServer
    {
        IPAddress Address { get; }
        int Port { get; }
        INetStatistics Stats { get; }
        IList<IClient> Clients { get; }
        IClient ServerClient { get; }

        void Connect(IPAddress address, ushort port);
        void Disconnect();
        IClient CreateClient();
        void DestroyClient(IClient cl);
        void DisconnectClient(IClient cl, string reason);
        void OnMessage(IMessage msg, ClientId cl);
        IClient IdToClient(ClientId id);
    }
}
