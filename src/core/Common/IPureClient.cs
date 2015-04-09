using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;

namespace Sdm.Core
{
    // represents local client (for client app)
    public interface IPureClient
    {
        event Action<ConnectionResult> ConnectionResult;

        ConnectionState ConnectionState { get; }
        ClientId Id { get; }
        IPAddress ServerAddress { get; }
        ushort ServerPort { get; }
        INetStatistics Stats { get; }
        // connection result have to be checked using ConnectionResult event
        void Connect(IPAddress address, ushort port, string login, string password);
        void Disconnect();
        void OnMessage(IMessage msg);
    }
}
