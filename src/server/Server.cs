using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using Sdm.Core;
using Sdm.Core.Crypto;
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
        public UserAccess AccessFlags { get; set; }
        public byte[] SessionKey { get; protected set; }
        public Stream NetStream { get { return Params.NetStream; } }
        #endregion
    }

    internal class Client : SocketClientBase
    {
        public Client(Server srv, ClientId id, SocketClientParams clParams, byte[] sessionKey) :
            base(srv, id, clParams, sessionKey)
        { AccessFlags = UserAccess.Default; }

        public override INetStatistics Stats { get { return null; } }
    }

    internal class Server : PureServerBase
    {
        private ServerConfig cfg;
        private UserList users;
        private Socket svSocket;
        private Thread acceptingThread;
        // client containers
        private readonly SortedList<ClientId, SocketClientBase> clients;
        private readonly ConcurrentQueue<SocketClientBase> newClients, delClients;
        private readonly List<IClient> iclients;
        private readonly ReadOnlyCollection<IClient> roClients;
        private readonly Dictionary<string, SocketClientBase> nameToClient;
        // ~client containers
        private readonly FileTransferSessionContainer ftsContainer;
        private readonly int MaxBlockSize;
        private const int MinBlockSize = 1024;
        private readonly ConcurrentQueue<IMessage> svcMessages;
        private readonly SemaphoreSlim semAcceptingThread;
        private volatile bool disconnecting = false;
        private IAsymmetricCryptoProvider asymCp;
        private ISymmetricCryptoProvider symCp;
        private RNGCryptoServiceProvider rng;
        private bool disposed = false;

        public ProtocolId Protocol { get; private set; }

        public Server(ServerConfig cfg)
        {
            this.cfg = cfg;
            users = new UserList();
            users.Load();
            clients = new SortedList<ClientId, SocketClientBase>();
            newClients = new ConcurrentQueue<SocketClientBase>();
            delClients = new ConcurrentQueue<SocketClientBase>();
            iclients = new List<IClient>();
            roClients = iclients.AsReadOnly();
            nameToClient = new Dictionary<string, SocketClientBase>();
            ftsContainer = new FileTransferSessionContainer();
            int minBufSize = Math.Min(cfg.SocketReceiveBufferSize, cfg.SocketSendBufferSize);
            MaxBlockSize = minBufSize / 2;
            svcMessages = new ConcurrentQueue<IMessage>();
            semAcceptingThread = new SemaphoreSlim(0, 1);
            Protocol = cfg.Protocol;
            asymCp = CryptoProviderFactory.Instance.CreateAsymmetric(cfg.AsymAlgorithm);
            asymCp.KeySize = cfg.AsymAlgorithmKeySize;
            symCp = CryptoProviderFactory.Instance.CreateSymmetric(cfg.SymAlgorithm);
            symCp.KeySize = cfg.SymAlgorithmKeySize;
            rng = new RNGCryptoServiceProvider();
        }

        public UserList Users { get { return users; } }

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
            var af = cfg.UseIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            svSocket = new Socket(af, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = cfg.SocketSendTimeout,
                SendBufferSize = cfg.SocketSendBufferSize,
                ReceiveBufferSize = cfg.SocketReceiveBufferSize
            };
            var localEp = new IPEndPoint(IPAddress.Any, port);
            try
            {
                svSocket.Bind(localEp);
                svSocket.Listen(cfg.SocketBacklog);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    Root.Log(LogLevel.Error, "Server: port {0} is busy!", port);
                Root.Log(LogLevel.Error, "Server: connection failed ({0})",
                    NetUtil.GetSocketErrorDesc(se.SocketErrorCode));
                throw;
            }
            Root.Log(LogLevel.Info, "Server: listening port " + port);
            StartAcceptLoop();
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
                Root.Log(LogLevel.Info, "Server: new client {0} ({1})", GetClientName(cl), cl.Address);
                AddClient(cl);
                var challenge = new SvPublicKeyChallenge {KeySize = asymCp.KeySize};
                try
                { SendTo(cl.Id, challenge); }
                catch (IOException e)
                {
                    if (NetUtil.CheckConnectionReset(e))
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
        
        private void ProcessSvcMessages()
        {
            IMessage msg;
            while (svcMessages.TryDequeue(out msg))
                OnMessage(msg, ClientId.Server);
        }

        private bool CheckClientConnection(SocketClientBase cl)
        {
            var s = cl.Params.Socket;
            return s.Connected && !(s.Poll(1, SelectMode.SelectRead) && s.Available == 0);
        }

        public override void Update()
        {
            var hdr = new MsgHeader();
            // try to read one message from each client
            foreach (var pair in clients)
            {
                var cl = pair.Value;
                if (!CheckClientConnection(cl))
                {
                    OnClientConnectionReset(cl);
                    continue;
                }
                if (cl.Params.Socket.Available == 0)
                    continue;
                if (!ReceiveMessageHeader(hdr, cl))
                    continue;
                if (hdr.Id.IsAuthRequired() && !cl.Authenticated)
                {
                    Root.Log(LogLevel.Error, "Discarding message [{0}] from non-authenticated client: {1}",
                        hdr.Id, GetClientName(cl));
                    DisconnectClient(cl, DisconnectReason.Unknown, "Can't process message without authentication");
                    continue;
                }
                var reqAccess = hdr.Id.GetRequiredAccess();
                var clAccess = cl.AccessFlags;
                if ((reqAccess & clAccess) != reqAccess)
                {
                    Root.Log(LogLevel.Error, "Discarding message [{0}] from client {1}: insufficient permissions",
                        hdr.Id, GetClientName(cl));
                    DisconnectClient(cl, DisconnectReason.Unknown, "Insufficient permissions");
                    continue;
                }
                IMessage msg;
                if (!ReceiveMessage(hdr, cl, out msg))
                    continue;
                OnMessage(msg, cl.Id);
            }
            ProcessDisconnectedClients();
            ProcessNewClients();
            ProcessSvcMessages();
        }

        private bool ReceiveMessageHeader(MsgHeader hdr, SocketClientBase cl)
        {
            try
            { hdr.Load(cl.NetStream, Protocol); }
            catch (MessageLoadException e)
            {
                if (NetUtil.CheckConnectionReset(e))
                    OnClientConnectionReset(cl);
                else
                {
                    Root.Log(LogLevel.Warning, "Client {0} : bad message header ({1})",
                        GetClientName(cl), e.Message);
                    DisconnectClient(cl, DisconnectReason.Unknown, "bad message header");
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
            { cl.NetStream.Read(buf, 0, buf.Length); }
            catch (IOException e)
            {
                if (NetUtil.CheckConnectionReset(e))
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
                    { msg.Load(msWrap, Protocol); }
                    catch (MessageLoadException e)
                    {
                        Root.Log(LogLevel.Warning, "Client {0} : bad message ({1})",
                            GetClientName(cl), e.Message);
                        DisconnectClient(cl, DisconnectReason.Unknown, "bad message");
                        return false;
                    }
                }
            }
            return true;
        }

        private void StartAcceptLoop()
        {
            acceptingThread = new Thread(AcceptLoopProc) {IsBackground = true};
            acceptingThread.Start();
        }

        private void AcceptLoopProc()
        {
            while (svSocket != null && svSocket.IsBound && !disconnecting)
            {
                Socket clSocket;
                try
                { clSocket = svSocket.Accept(); }
                catch (SocketException) // Disconnect() called
                { break; }
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
            if (disconnecting || !Connected)
                return;
            var msg = new ClDisconnect();
            svcMessages.Enqueue(msg);
        }

        private void InternalDisconnect()
        {
            disconnecting = true;
            Root.Log(LogLevel.Info, "Server: disconnecting");
            foreach (var cl in iclients)
                DisconnectClient(cl, DisconnectReason.Shutdown, "Server stopped");
            ProcessDisconnectedClients();
            if (svSocket != null)
            {
                if (svSocket.Connected)
                    svSocket.Shutdown(SocketShutdown.Both);
                svSocket.Close();
                svSocket = null;
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

        public override void DisconnectClient(IClient cl, DisconnectReason reason, string message = "")
        {
            var msg = new SvDisconnect {Message = message, Reason = reason};
            SendTo(cl.Id, msg);
            var scl = clients[cl.Id];
            DisconnectClient(scl);
        }

        private void DisconnectClient(SocketClientBase cl)
        {
            Root.Log(LogLevel.Info, "Server: disconnecting client: " + GetClientName(cl));
            cl.DeferredDisconnect = true;
            delClients.Enqueue(cl);
            cl.Params.Socket.Shutdown(SocketShutdown.Both);
            cl.Params.Socket.Close();
            if (cl.Authenticated)
            {
                var msg = new SvUserlistUpdate();
                msg.Disconnected = new[] {cl.Login};
                SendBroadcast(cl.Id, msg);
            }
        }

        private AuthResult AuthenticateClient(string login, string password, ref UserAccess accessFlags)
        {
            if (nameToClient.ContainsKey(login))
                return AuthResult.AlreadyLoggedIn;
            var user = users.Find(login);
            if (user == null)
                return AuthResult.InvalidLogin;
            if (!user.VerifyPassword(password))
                return AuthResult.InvalidLogin;
            accessFlags = user.Access;
            return AuthResult.Accepted;
        }

        public override void OnMessage(IMessage msg, ClientId cl)
        {
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
            case MessageId.ClUserlistRequest:
                OnClUserlistRequest(msg as ClUserlistRequest, cl);
                break;
            case MessageId.CsChatMessage:
                OnCsChatMessage(msg as CsChatMessage, cl);
                break;
            case MessageId.ClFileTransferRequest:
                OnClFileTransferRequest(msg as ClFileTransferRequest, cl);
                break;
            case MessageId.ClFileTransferRespond:
                OnClFileTransferRespond(msg as ClFileTransferRespond, cl);
                break;
            case MessageId.CsFileTransferData:
                OnCsFileTransferData(msg as CsFileTransferData, cl);
                break;
            case MessageId.CsFileTransferVerificationResult:
                OnCsFileTransferVerificationResult(msg as CsFileTransferVerificationResult, cl);
                break;
            case MessageId.CsFileTransferInterruption:
                OnCsFileTransferInterruption(msg as CsFileTransferInterruption, cl);
                break;
            }
        }

        private void OnClAuthRespond(ClAuthRespond msg, ClientId id)
        {
            var cl = clients[id];
            var accessFlags = UserAccess.Default;
            var result = AuthenticateClient(msg.Login, msg.Password, ref accessFlags);
            var respond = new SvAuthResult { Result = result };
            try
            {
                if (result == AuthResult.Accepted)
                {
                    cl.Login = msg.Login;
                    cl.Password = msg.Password;
                    cl.AccessFlags = accessFlags;
                    cl.Authenticated = true;
                    nameToClient.Add(cl.Login, cl);
                    respond.Message = "All ok";
                    SendTo(id, respond);
                    Root.Log(LogLevel.Info, "Client #{0} ({1}) : authentication succeeded", cl.Id, cl.Login);
                    OnClientAuthSuccess(id);
                }
                else
                {
                    // XXX: add extra details here
                    respond.Message = "";
                    SendTo(id, respond);
                    Root.Log(LogLevel.Info, "Client #{0} ({1}) : authentication failed <{2}>",
                        cl.Id, msg.Login, result);
                    DisconnectClient(cl);
                }
            }
            catch (IOException e)
            {
                if (NetUtil.CheckConnectionReset(e))
                    OnClientConnectionReset(cl);
                else
                    throw;
            }
        }

        private void OnClientAuthSuccess(ClientId id)
        {
            // XXX: could be optimized: collect all userlist updates and broadcast merged
            // update once all clients are updated
            var msg = new SvUserlistUpdate();
            msg.Connected = new[] {clients[id].Login};
            SendBroadcast(id, msg);
        }

        private void OnClPublicKeyRespond(ClPublicKeyRespond msg, ClientId id)
        {
            var cl = clients[id];
            asymCp.SetKey(msg.Key);
            var encryptedKey = asymCp.Encrypt(cl.SessionKey);
            var challenge = new SvAuthChallenge {SessionKey = encryptedKey};
            try
            { SendTo(id, challenge); }
            catch (IOException e)
            {
                if (NetUtil.CheckConnectionReset(e))
                    OnClientConnectionReset(cl);
                else
                    throw;
            }
            cl.Secure = true;
        }

        private void OnClDisconnect(ClDisconnect msg, ClientId id)
        {
            if (id == ClientId.Server)
            {
                InternalDisconnect();
                return;
            }
            var cl = clients[id];
            Root.Log(LogLevel.Info, "Client {0} : disconnect", GetClientName(cl));
            DisconnectClient(cl);
        }

        private void OnClUserlistRequest(ClUserlistRequest msg, ClientId id)
        {
            var unames = new string[clients.Count];
            for (int i = 0; i < clients.Count; i++)
                unames[i] = clients.Values[i].Login;
            var respond = new SvUserlistRespond {Usernames = unames};
            SendTo(id, respond);
        }

        private void OnCsChatMessage(CsChatMessage msg, ClientId id)
        {
            var sender = clients[id];
            SocketClientBase receiver;
            if (nameToClient.TryGetValue(msg.Username, out receiver))
            {
                msg.Username = sender.Login;
                SendTo(receiver.Id, msg);
            }
        }

        private void OnClFileTransferRequest(ClFileTransferRequest msg, ClientId id)
        {
            var sender = clients[id];
            SocketClientBase receiver;
            if (!nameToClient.TryGetValue(msg.Username, out receiver))
            {
                Root.Log(LogLevel.Warning, "Server: client {0} requested file transfer " +
                    "to unknown client {1}", id, msg.Username);
                var result = new SvFileTransferResult
                {
                    Result = FileTransferRequestResult.Rejected,
                    SessionId = FileTransferId.InvalidId,
                    Token = msg.Token
                };
                SendTo(id, result);
                return;
            }
            var ft = ftsContainer.CreateSession(msg.Token, sender.Login, receiver.Login, msg.FileHash, msg.FileSize);
            ft.SrcName = msg.FileName;
            ft.BlockSize = SelectBlockSize(msg.BlockSize);
            var request = new SvFileTransferRequest
            {
                Username = sender.Login,
                FileName = msg.FileName,
                FileHash = msg.FileHash,
                FileSize = msg.FileSize,
                BlockSize = ft.BlockSize,
                SessionId = ft.Id
            };
            SendTo(receiver.Id, request);
        }

        private void OnClFileTransferRespond(ClFileTransferRespond msg, ClientId id)
        {
            var cl = clients[id];
            var ft = ftsContainer.GetSessionById(msg.SessionId);
            if (ft == null)
            {
                Root.Log(LogLevel.Warning, "Server: bad file transfer session id ({0}) received from client {1} ",
                    msg.SessionId, cl.Login);
                // XXX: send result
                return;
            }
            SocketClientBase fileSender;
            if (!nameToClient.TryGetValue(ft.Sender, out fileSender))
            {
                Root.Log(LogLevel.Warning, "Server: client {0} attempted to send file transfer verification result " +
                    "to disconnected client {1}", cl.Login, ft.Receiver);
                // XXX: suspend session (cl is offline)
                return;
            }
            var result = new SvFileTransferResult
            {
                Result = msg.Result,
                Token = ft.Token
            };
            if (msg.Result == FileTransferRequestResult.Accepted)
            {
                var newBlockSize = Math.Min(SelectBlockSize(msg.BlockSize), ft.BlockSize);
                ft.BlockSize = newBlockSize;
                ft.State = FileTransferState.Working;
                result.BlockSize = newBlockSize;
                result.SessionId = msg.SessionId;
            }
            else
            {
                ftsContainer.DeleteSession(ft.Id);
                result.SessionId = FileTransferId.InvalidId;
            }
            SendTo(fileSender.Id, result);
        }

        private void OnCsFileTransferData(CsFileTransferData msg, ClientId id)
        {
            var cl = clients[id];
            var ft = ftsContainer.GetSessionById(msg.SessionId);
            if (ft == null)
            {
                Root.Log(LogLevel.Warning, "Server: bad file transfer session id ({0}) received from client {1} ",
                    msg.SessionId, cl.Login);
                // XXX: send result
                return;
            }
            SocketClientBase receiver;
            if (!nameToClient.TryGetValue(ft.Receiver, out receiver))
            {
                Root.Log(LogLevel.Warning, "Server: ignoring file transfer data {0} -> {1} (receiver disconnected)",
                    cl.Login, ft.Receiver);
                // XXX: suspend session (cl is offline)
                return;
            }
            if (ft.State != FileTransferState.Working)
            {
                Root.Log(LogLevel.Error, "Server: invalid file transfer state [sid={0}, expected={1}, got={2}]",
                    ft.Id, FileTransferState.Working, ft.State);
                return;
            }
            SendTo(receiver.Id, msg);
            ft.BlocksDone++;
            if (ft.BlocksDone == ft.BlocksTotal)
                ft.State = FileTransferState.Verification;
        }

        private void OnCsFileTransferVerificationResult(CsFileTransferVerificationResult msg, ClientId id)
        {
            var cl = clients[id];
            var ft = ftsContainer.GetSessionById(msg.SessionId);
            if (ft == null)
            {
                Root.Log(LogLevel.Warning, "Server: bad file transfer session id ({0}) received from client {1} ",
                    msg.SessionId, cl.Login);
                // XXX: send result
                return;
            }
            if (ft.State != FileTransferState.Verification)
            {
                Root.Log(LogLevel.Error, "Server: invalid file transfer state [sid={0}, expected={1}, got={2}]",
                    ft.Id, FileTransferState.Verification, ft.State);
                return;
            }
            SocketClientBase fileSender;
            if (!nameToClient.TryGetValue(ft.Sender, out fileSender))
            {
                Root.Log(LogLevel.Warning, "Server: client {0} attempted to send file transfer verification result " +
                    "to disconnected client {1}", cl.Login, ft.Receiver);
                // XXX: suspend session (cl is offline)
                return;
            }
            if (msg.Result == FileTransferVerificationResult.Success)
                ft.State = FileTransferState.Success;
            else
                ft.State = FileTransferState.Failure;
            ftsContainer.DeleteSession(ft.Id);
            SendTo(fileSender.Id, msg);
        }

        private void OnCsFileTransferInterruption(CsFileTransferInterruption msg, ClientId id)
        {
            var sender = clients[id];
            FileTransferSession ft = null;
            if (msg.SessionId != FileTransferId.InvalidId)
                ft = ftsContainer.GetSessionById(msg.SessionId);
            else // client is sender and doesn't have sid yet
            {
                var senderSessions = ftsContainer.GetUserSessions(sender.Login);
                if (senderSessions != null)
                {
                    foreach (var sid in senderSessions)
                    {
                        var s = ftsContainer.GetSessionById(sid);
                        if (s.Token == msg.Token && s.Sender == sender.Login)
                        {
                            ft = s;
                            break;
                        }
                    }
                }
                if (ft == null)
                {
                    Root.Log(LogLevel.Warning, "Server: ignoring file transfer interruption from client {0} " +
                        "- session not found", sender.Login);
                    return;
                }
            }
            string oppUsername = sender.Login == ft.Sender ? ft.Receiver : ft.Sender;
            SocketClientBase oppClient;
            if (!nameToClient.TryGetValue(oppUsername, out oppClient))
            {
                Root.Log(LogLevel.Warning, "Server: client {0} attempted to send file transfer data " +
                    "to disconnected client {1}", ft.Sender, ft.Receiver);
                // XXX: suspend session (cl is offline)
                return;
            }
            msg.SessionId = ft.Id;
            switch (msg.Int)
            {
            case FileTransferInterruption.Cancel:
            default:
                ft.State = FileTransferState.Cancelled;
                ftsContainer.DeleteSession(ft.Id);
                break;
            }
            SendTo(oppClient.Id, msg);
        }

        private int SelectBlockSize(int clSize)
        {
            int blockSize = Math.Min(MaxBlockSize, clSize);
            if (blockSize < MinBlockSize)
                blockSize = MinBlockSize;
            return blockSize;
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
            using (var rawBuf = new MemoryStream())
            {
                var buf = rawBuf.AsUnclosable();
                var header = new MsgHeader {Id = msg.Id, Flags = MessageFlags.None};
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
                // XXX: handle exceptions
                // exception will be thrown here if client was disconnected ungracefully
                header.Save(cl.NetStream, Protocol);
                rawBuf.WriteTo(cl.NetStream);
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
            if (cl.Login != null)
                nameToClient.Remove(cl.Login);
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
                    if (svSocket != null)
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
