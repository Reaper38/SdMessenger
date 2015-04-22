﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
        public bool Secure { get; set; }
        public bool Authenticated { get; set; }
        public bool DeferredDisconnect { get; set; }
        public ClientAccessFlags AccessFlags { get; set; }
        public byte[] SessionKey { get; protected set; }
        public Stream NetStream { get { return Params.NetStream; } }
        #endregion
    }

    internal class Client : SocketClientBase
    {
        public Client(Server srv, ClientId id, SocketClientParams clParams, byte[] sessionKey) :
            base(srv, id, clParams, sessionKey)
        {
            AccessFlags = ClientAccessFlags.Default;
        }

        public override INetStatistics Stats { get { return null; } }
    }

    internal class Server : PureServerBase
    {
        private Socket svSocket;
        private Thread acceptingThread;
        private readonly SortedList<ClientId, SocketClientBase> clients;
        private readonly ConcurrentQueue<SocketClientBase> newClients, delClients;
        private readonly List<IClient> iclients;
        private readonly ReadOnlyCollection<IClient> roClients;
        private readonly SemaphoreSlim semAcceptingThread;
        private volatile bool disconnecting = false;
        private IAsymmetricCryptoProvider asymCp;
        private ISymmetricCryptoProvider symCp;
        private RNGCryptoServiceProvider rng;
        private bool disposed = false;

        public ProtocolId Protocol { get; private set; }

        public Server()
        {
            clients = new SortedList<ClientId, SocketClientBase>();
            newClients = new ConcurrentQueue<SocketClientBase>();
            delClients = new ConcurrentQueue<SocketClientBase>();
            iclients = new List<IClient>();
            roClients = iclients.AsReadOnly();
            semAcceptingThread = new SemaphoreSlim(0, 1);
            // XXX: load protocol id from config
            Protocol = ProtocolId.Json;
            asymCp = CryptoProviderFactory.Instance.CreateAsymmetric(SdmAsymmetricAlgorithm.RSA);
            symCp = CryptoProviderFactory.Instance.CreateSymmetric(SdmSymmetricAlgorithm.AES);
            rng = new RNGCryptoServiceProvider();
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
            Root.Log(LogLevel.Info, "Server: starting...");
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
                    Root.Log(LogLevel.Error, "Server: port {0} is busy!", port);
                }
                Root.Log(LogLevel.Error, "Server: connection failed ({0})",
                    NetUtil.GetSocketErrorDesc(se.SocketErrorCode));
                throw;
            }
            Root.Log(LogLevel.Info, "Server: listening port " + port);
            StartAcceptLoop();
        }

        /// <summary>Check if the base exception is SocketException with ConnectionReset error code.</summary>
        private static bool CheckConnectionReset(Exception e)
        {
            var se = e.GetBaseException() as SocketException;
            return se != null && se.SocketErrorCode == SocketError.ConnectionReset;
        }

        private void OnClientConnectionReset(SocketClientBase cl)
        {
            Root.Log(LogLevel.Info, "Client {0} : connection lost", GetClientName(cl));
            DisconnectClient(cl);
        }

        private void ProcessNewClients()
        {
            SocketClientBase cl;
            while (newClients.TryDequeue(out cl))
            {
                AddClient(cl);
                var challenge = new SvPublicKeyChallenge { KeySize = asymCp.KeySize };
                try
                {
                    SendTo(cl.Id, challenge);
                }
                catch (IOException e)
                {
                    if (CheckConnectionReset(e))
                        OnClientConnectionReset(cl);
                    else
                        throw;
                }
            }
        }

        private void ProcessDisconnectedClients()
        {
            SocketClientBase cl;
            while (delClients.TryDequeue(out cl))
                RemoveClient(cl);
        }
        
        public override void Update()
        {
            var hdr = new MsgHeader();
            // try to read one message from each client
            foreach (var pair in clients)
            {
                var cl = pair.Value;
                if (!cl.Params.Socket.Connected)
                {
                    OnClientConnectionReset(cl);
                    continue;
                }
                if (cl.Params.Socket.Available == 0)
                    continue;
                if (!ReceiveMessageHeader(hdr, cl))
                    continue;
                IMessage msg;
                if (!ReceiveMessage(hdr, cl, out msg))
                    continue;
                OnMessage(msg, cl.Id);
            }
            ProcessDisconnectedClients();
            ProcessNewClients();
        }

        private bool ReceiveMessageHeader(MsgHeader hdr, SocketClientBase cl)
        {
            try
            {
                hdr.Load(cl.NetStream, Protocol);
            }
            catch (MessageLoadException e)
            {
                if (CheckConnectionReset(e))
                    OnClientConnectionReset(cl);
                else
                {
                    Root.Log(LogLevel.Warning, "Client {0} : bad message header ({1})",
                        GetClientName(cl), e.Message);
                    DisconnectClient(cl, "bad message header");
                }
                return false;
            }
            return true;
        }

        private bool ReceiveMessage(MsgHeader hdr, SocketClientBase cl, out IMessage msg)
        {
            msg = null;
            // XXX: could be optimized - use one large buffer + unclosable MemoryStream
            var buf = new byte[hdr.Size];
            try
            {
                cl.NetStream.Read(buf, 0, buf.Length);
            }
            catch (IOException e)
            {
                if (CheckConnectionReset(e))
                {
                    OnClientConnectionReset(cl);
                    return false;
                }
                throw;
            }
            using (var ms = new MemoryStream(buf))
            {
                var msWrap = ms.AsUnclosable();
                if ((hdr.Flags & MessageFlags.Secure) == MessageFlags.Secure)
                {
                    using (var container = new MessageCryptoContainer())
                    {
                        container.Load(msWrap, Protocol);
                        symCp.Key = cl.SessionKey;
                        msg = container.Extract(hdr.Id, symCp, Protocol);
                    }
                }
                else
                {
                    msg = MessageFactory.CreateMessage(hdr.Id);
                    try
                    {
                        msg.Load(msWrap, Protocol);
                    }
                    catch (MessageLoadException e)
                    {
                        Root.Log(LogLevel.Warning, "Client {0} : bad message ({1})",
                            GetClientName(cl), e.Message);
                        DisconnectClient(cl, "bad message");
                        return false;
                    }
                }
            }
            return true;
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
                catch (ObjectDisposedException) // Disconnect() called
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
                    var cl = CreateClient(clParams);
                    newClients.Enqueue(cl);
                }
                else
                {
                    clSocket.Shutdown(SocketShutdown.Both);
                    clSocket.Close();
                }
            }
            Root.Log(LogLevel.Info, "Server: disconnected");
            if (semAcceptingThread.CurrentCount == 0)
                semAcceptingThread.Release();
        }

        protected override void OnNewClient(IClientParams clParams, ref bool allow) {}

        public override void Disconnect()
        {
            disconnecting = true;
            Root.Log(LogLevel.Info, "Server: disconnecting");
            foreach (var cl in iclients)
                DisconnectClient(cl, "Server stopped");
            ProcessDisconnectedClients();
            if (svSocket != null)
            {
                svSocket.Shutdown(SocketShutdown.Both);
                svSocket.Close();
            }
            semAcceptingThread.Wait();
            disconnecting = false;
        }

        private SocketClientBase CreateClient(SocketClientParams clParams)
        {
            var id = new ClientId(clParams);
            var cl = new Client(this, id, clParams, GenerateSessionKey());
            return cl;
        }

        private static string GetClientName(IClient cl)
        { return cl.Login ?? "#" + cl.Id; }

        public override void DisconnectClient(IClient cl, string reason)
        {
            var scl = clients[cl.Id];
            // XXX: send notification to client
            DisconnectClient(scl);
        }

        private void DisconnectClient(SocketClientBase cl)
        {
            Root.Log(LogLevel.Info, "Server: disconnecting client: " + GetClientName(cl));
            cl.DeferredDisconnect = true;
            delClients.Enqueue(cl);
            cl.Params.Socket.Shutdown(SocketShutdown.Both);
            cl.Params.Socket.Close();
        }

        private AuthResult AuthenticateClient(string login, string password, ref ClientAccessFlags accessFlags)
        {
            // XXX: implement account verification
            // 1] check if user with specified login exists in account db
            // 2] calculate password hash
            // 3] check if calculated hash matches one in account db
            // 4] get access flags
            return AuthResult.Accepted;
        }

        public override void OnMessage(IMessage msg, ClientId cl)
        {
            // XXX: handle messages here
            switch (msg.Id)
            {
            case MessageId.ClPublicKeyRespond:
                OnClPublicKeyRespond(msg as ClPublicKeyRespond, cl);
                break;
            case MessageId.ClAuthRespond:
                OnClAuthRespond(msg as ClAuthRespond, cl);
                break;
            case MessageId.ClDisconnect:
                OnClDisconnect(msg as ClDisconnect, cl);
                break;
            }
        }

        private void OnClAuthRespond(ClAuthRespond msg, ClientId id)
        {
            var cl = clients[id];
            var accessFlags = ClientAccessFlags.Default;
            var result = AuthenticateClient(msg.Login, msg.Password, ref accessFlags);
            var respond = new SvAuthResult { Result = result };
            try
            {
                if (result == AuthResult.Accepted)
                {
                    cl.Login = msg.Login;
                    cl.Password = msg.Password;
                    cl.Authenticated = true;
                    respond.Message = "All ok";
                    SendTo(id, respond);
                    Root.Log(LogLevel.Info, "Client {0} : authentication succeeded", cl.Login);
                }
                else
                {
                    // XXX: add extra details here
                    respond.Message = "";
                    SendTo(id, respond);
                    Root.Log(LogLevel.Info, "Client {0} : authentication failed", cl.Login);
                    DisconnectClient(cl);
                }
            }
            catch (IOException e)
            {
                if (CheckConnectionReset(e))
                    OnClientConnectionReset(cl);
                else
                    throw;
            }
        }

        private void OnClPublicKeyRespond(ClPublicKeyRespond msg, ClientId id)
        {
            var cl = clients[id];
            asymCp.SetKey(msg.Key);
            var encryptedKey = asymCp.Encrypt(cl.SessionKey);
            var challenge = new SvAuthChallenge { SessionKey = encryptedKey };
            try
            {
                SendTo(id, challenge);
            }
            catch (IOException e)
            {
                if (CheckConnectionReset(e))
                    OnClientConnectionReset(cl);
                else
                    throw;
            }
            cl.Secure = true;
        }

        private void OnClDisconnect(ClDisconnect msg, ClientId id)
        {
            var cl = clients[id];
            Root.Log(LogLevel.Info, "Client {0} : disconnect", cl.Login);
            DisconnectClient(cl);
            // XXX: broadcast event
        }

        public override IClient IdToClient(ClientId id)
        { return clients[id]; }
        
        public override void SendTo(ClientId id, IMessage msg)
        {
            var cl = clients[id];
            if (cl.DeferredDisconnect)
            {
                Root.Log(LogLevel.Debug, "Server: attempt to send message to disconnected client");
                return;
            }
            using (var buf = new MemoryStream())
            {
                var header = new MsgHeader();
                header.Id = msg.Id;
                header.Flags = MessageFlags.None;
                if (cl.Secure)
                {
                    using (var container = new MessageCryptoContainer())
                    {
                        symCp.Key = cl.SessionKey;
                        // XXX: generate new symCp.IV
                        container.Store(msg, symCp, Protocol);
                        container.Save(buf, Protocol);
                        header.Size = (int) buf.Length;
                        header.Flags |= MessageFlags.Secure;
                    }
                }
                else
                {
                    msg.Save(buf, Protocol);
                    header.Size = (int)buf.Length;
                }
                header.Save(cl.NetStream, Protocol);
                buf.WriteTo(cl.NetStream);
            }
        }

        public override void SendBroadcast(ClientId exclude, IMessage msg, bool authenticatedOnly = true)
        {
            foreach (var cl in iclients)
            {
                if (cl.Id != exclude && (!authenticatedOnly || cl.Authenticated))
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
            var keySize = symCp.KeySize / 8;
            var key = new byte[keySize];
            rng.GetBytes(key);
            return key;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    svSocket.Dispose();
                    asymCp.Dispose();
                    symCp.Dispose();
                    rng.Dispose();
                }
                DisposeHelper.OnDispose<Server>(disposing);
                disposed = true;
            }
        }
    }
}
