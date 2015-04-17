using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sdm.Core;
using Sdm.Core.Messages;
using Sdm.Core.Util;

namespace Sdm.Server
{
    internal class SocketClientParams : IClientParams
    {
        public SocketClientParams(Socket clSocket, Stream netStream)
        {
            Socket = clSocket;
            NetStream = netStream;
        }

        public Socket Socket { get; protected set; }
        public Stream NetStream { get; protected set; }
        public IPAddress Address { get { return ((IPEndPoint)Socket.RemoteEndPoint).Address; } }
        public int Port { get { return ((IPEndPoint)Socket.RemoteEndPoint).Port; } }
    }
    
    internal abstract class SocketClientBase : IClient
    {
        protected PureServerBase Server;
        public SocketClientParams Params { get; protected set; }

        protected SocketClientBase(PureServerBase srv, ClientId id, SocketClientParams clParams, byte[] sessionKey)
        {
            Server = srv;
            Params = clParams;
            Id = id;
            SessionKey = sessionKey;
        }

        #region IClient Members
        public ClientId Id { get; private set; }
        public IPAddress Address { get { return Params.Address; } }
        public ushort Port { get { return (ushort)Params.Port; } }
        public abstract INetStatistics Stats { get; }
        public string Login { get; set; }
        public string Password { get; set; }
        public abstract ClientFlags Flags { get; protected set; }
        public ClientAccessFlags AccessFlags { get; set; }
        public byte[] SessionKey { get; protected set; }
        public Stream NetStream { get { return Params.NetStream; } }
        #endregion
    }

    internal class Client : SocketClientBase
    {
        private ClientFlags flags;

        public Client(Server srv, ClientId id, SocketClientParams clParams, byte[] sessionKey) :
            base(srv, id, clParams, sessionKey)
        {
            flags = ClientFlags.None;
            AccessFlags = ClientAccessFlags.Default;
        }

        public override INetStatistics Stats { get { return null; } }

        public override ClientFlags Flags
        {
            get { return flags; }
            protected set { flags = value; }
        }
    }

    internal class Server : PureServerBase
    {
        private Socket svSocket;
        private Thread acceptingThread;
        private readonly SortedList<ClientId, SocketClientBase> clients;
        private readonly List<IClient> iclients;
        private readonly ReadOnlyCollection<IClient> roClients;
        private readonly SemaphoreSlim semAcceptingThread;
        private volatile bool disconnecting = false;

        public ProtocolId Protocol { get; private set; }

        public Server()
        {
            clients = new SortedList<ClientId, SocketClientBase>();
            iclients = new List<IClient>();
            roClients = iclients.AsReadOnly();
            semAcceptingThread = new SemaphoreSlim(0, 1);
            // XXX: load protocol id from config
            Protocol = ProtocolId.Json;
        }
        
        #region PureServerBase Members

        public override bool Connected
        { get { return svSocket != null && svSocket.IsBound; } }

        public override IPAddress Address
        {
            get
            {
                if (Connected)
                    return ((IPEndPoint)svSocket.LocalEndPoint).Address;
                return IPAddress.None;
            }
        }

        public override int Port
        {
            get
            {
                if (Connected)
                    return ((IPEndPoint)svSocket.LocalEndPoint).Port;
                return 0;
            }
        }

        public override INetStatistics Stats
        { get { return null; } }

        public override IList<IClient> Clients { get { return roClients; } }
        
        public override void Connect(IPAddress address, ushort port)
        {
            if (Connected)
                throw new InvalidOperationException("Already connected");
            // XXX: get socket params from config
            svSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 1000,
                SendBufferSize = 0x8000,
                ReceiveBufferSize = 0x8000
            };
            var localEp = new IPEndPoint(IPAddress.Any, port);
            try
            {
                svSocket.Bind(localEp);
                // XXX: get backlog count from config
                svSocket.Listen(4);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    // XXX: log 'port is busy'
                }
                // XXX: log failure
                throw;
            }
            StartAcceptLoop();
        }

        public override void Update()
        {
            var hdr = new MsgHeader();
            foreach (var pair in clients)
            {
                var cl = pair.Value;
                if (cl.Params.Socket.Available == 0)
                    continue;
                // try to read one message from each client
                try
                {
                    hdr.Load(cl.NetStream, Protocol);
                }
                catch (MessageLoadException e)
                {
                    // XXX: log exception
                    DisconnectClient(cl, "bad message header");
                    continue;
                }
                // XXX: could be optimized - use one large buffer + unclosable MemoryStream
                var buf = new byte[hdr.Size];
                cl.NetStream.Read(buf, 0, buf.Length);
                var ms = new MemoryStream(buf);
                var msg = MessageFactory.CreateMessage(hdr.Id);
                        try
                        {
                            msg.Load(ms, Protocol);
                        }
                        catch (MessageLoadException e)
                        {
                            // XXX: log exception
                            DisconnectClient(cl, "bad message");
                            continue;
                        }
                OnMessage(msg, cl.Id);
            }
        }

        private void StartAcceptLoop()
        {
            acceptingThread = new Thread(AcceptLoopProc) { IsBackground = true };
            acceptingThread.Start();
        }

        private void AcceptLoopProc()
        {
            while (svSocket.IsBound && !disconnecting)
            {
                Socket clSocket;
                try
                {
                    clSocket = svSocket.Accept();
                }
                catch
                {
                    break;
                }
                if (disconnecting)
                    break;
                var rawStream = new NetworkStream(clSocket, false);
                var clParams = new SocketClientParams(clSocket, rawStream.AsUnclosable());
                var allow = true;
                OnNewClient(clParams, ref allow);
                if (allow)
                {
                    // XXX: log 'connection accepted'
                    var cl = CreateClient(clParams);
                    AddClient(cl);
                }
                else
                {
                    // XXX: log 'connection rejected'
                }
            }
            // XXX: log 'disconnected'
            if (semAcceptingThread.CurrentCount == 0)
                semAcceptingThread.Release();
        }

        protected override void OnNewClient(IClientParams clParams, ref bool allow) {}

        public override void Disconnect()
        {
            disconnecting = true;
            // XXX: log 'disconnect'
            foreach (var cl in iclients)
                DisconnectClient(cl, "Server stopped");
            if (svSocket != null)
                svSocket.Close();
            semAcceptingThread.Wait();
            disconnecting = false;
        }

        private SocketClientBase CreateClient(SocketClientParams clParams)
        {
            var id = new ClientId(clParams);
            var cl = new Client(this, id, clParams, GenerateSessionKey());
            return cl;
        }

        public override void DisconnectClient(IClient cl, string reason)
        {
            var scl = clients[cl.Id];
            // XXX: send notification to client
            RemoveClient(scl);
            scl.Params.Socket.Close();
        }

        public override void OnMessage(IMessage msg, ClientId cl)
        {
            // XXX: handle messages here
        }

        public override IClient IdToClient(ClientId id)
        { return clients[id]; }
        
        public override void SendTo(ClientId id, IMessage msg)
        {
            var cl = clients[id];
            msg.Save(cl.NetStream, Protocol);
        }

        public override void SendBroadcast(ClientId exclude, IMessage msg)
        {
            foreach (var cl in iclients)
            {
                if (cl.Id != exclude)
                    SendTo(cl.Id, msg);
            }
        }
        
        #endregion
        
        private void AddClient(SocketClientBase cl)
        {
            clients.Add(cl.Id, cl);
            iclients.Add(cl);
        }

        private void RemoveClient(SocketClientBase cl)
        {
            clients.Remove(cl.Id);
            iclients.Remove(cl);
        }

        private byte[] GenerateSessionKey()
        {
            // XXX: generate session key
            return new byte[16];
        }
    }
}
