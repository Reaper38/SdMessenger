﻿using System;
using System.Collections.Generic;
using System.Net;

namespace Sdm.Core
{
    // represents server by itself (for server app)
    public abstract class PureServerBase
    {
        public abstract bool Connected { get; }
        public abstract IPAddress Address { get; }
        public abstract int Port { get; }
        public abstract INetStatistics Stats { get; }
        public abstract IList<IClient> Clients { get; }
        public abstract void Connect(IPAddress address, ushort port);
        public abstract void Disconnect();
        public abstract void DisconnectClient(IClient cl, string reason);
        public abstract void Update();
        public abstract void OnMessage(IMessage msg, ClientId sender);
        public abstract void SendTo(ClientId id, IMessage msg);
        public abstract void SendBroadcast(ClientId exclude, IMessage msg, bool authenticatedOnly = true);
        public abstract IClient IdToClient(ClientId id);
        protected abstract void OnNewClient(IClientParams clParams, ref bool allow);
    }

    public interface IClientParams
    {
        IPAddress Address { get; }
        int Port { get; }
    }
}
