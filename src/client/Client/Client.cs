using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Sdm.Core;
using Sdm.Core.Crypto;
using Sdm.Core.Messages;
using Sdm.Core.Util;

namespace Sdm.Client
{
    internal class Client : PureClientBase
    {
        private Socket clSocket;
        private NetworkStream rawNetStream;
        private Stream netStream; // unclosable
        private byte[] sessionKey; // null == no session key has been received yet
        private string login, password;
        private bool authenticated;
        private bool disconnectReceived;
        private readonly ClientConfig cfg;
        private IAsymmetricCryptoProvider asymCp;
        private ISymmetricCryptoProvider symCp;
        private bool disposed = false;

        public Client(ClientConfig cfg)
        {
            this.cfg = cfg;
            asymCp = CryptoProviderFactory.Instance.CreateAsymmetric(cfg.AsymAlgorithm);
            symCp = CryptoProviderFactory.Instance.CreateSymmetric(cfg.SymAlgorithm);
        }
        
        public override IPAddress ServerAddress
        {
            get
            {
                if (Connected)
                    return ((IPEndPoint)clSocket.RemoteEndPoint).Address;
                return IPAddress.None;
            }
        }

        public override ushort ServerPort
        {
            get
            {
                if (Connected)
                    return (ushort)((IPEndPoint)clSocket.RemoteEndPoint).Port;
                return 0;
            }
        }

        public override INetStatistics Stats { get { return null; } }
        
        public override void Connect(IPAddress address, ushort port, string login, string password)
        {
            if (ConnectionState != ConnectionState.Disconnected)
                throw new InvalidOperationException("Already connected or waiting");
            this.login = login;
            this.password = password;
            ConnectionState = ConnectionState.Waiting;
            Root.Log(LogLevel.Info, "Client: connecting to {0}:{1} ...", address, port);
            var af = cfg.UseIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            clSocket = new Socket(af, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = cfg.SocketSendTimeout,
                SendBufferSize = cfg.SocketSendBufferSize,
                ReceiveBufferSize = cfg.SocketReceiveBufferSize
            };
            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(address, port);
            args.Completed += EndConnect;
            // XXX: handle exceptions thrown by ConnectAsync
            if (!clSocket.ConnectAsync(args))
                EndConnect(null, args);
        }

        private void Reset()
        {
            var prevState = ConnectionState;
            ConnectionState = ConnectionState.Disconnected;
            authenticated = false;
            disconnectReceived = false;
            sessionKey = null;
            netStream = null;
            if (rawNetStream != null)
            {
                rawNetStream.Close();
                rawNetStream = null;
            }
            if (clSocket != null)
            {
                clSocket.Close();
                clSocket = null;
            }
            if (prevState == ConnectionState.Connected)
                Root.Log(LogLevel.Info, "Client: disconnected");
        }

        private void EndConnect(object sender, SocketAsyncEventArgs e)
        {
            var err = e.SocketError;
            e.Dispose();
            ConnectionResult cr;
            string msg;
            if (err != SocketError.Success)
            {
                cr = Core.ConnectionResult.Rejected;
                msg = NetUtil.GetSocketErrorDesc(err);
                Root.Log(LogLevel.Error, "Client: connection failed: " + msg);
                Reset();
            }
            else
            {
                cr = Core.ConnectionResult.Accepted;
                msg = "Connection established";
                Root.Log(LogLevel.Info, "Client: connection established");
                ConnectionState = ConnectionState.Connected;
                rawNetStream = new NetworkStream(clSocket);
                netStream = rawNetStream.AsUnclosable();
            }
            OnConnectionResult(cr, msg);
        }

        public override void Disconnect()
        {
            Root.Log(LogLevel.Info, "Client: disconnect");
            if (!disconnectReceived)
            {
                var msg = new ClDisconnect();
                Send(msg);
            }
            clSocket.Shutdown(SocketShutdown.Both);
            Reset();
        }

        public override void Update()
        {
            var hdr = new MsgHeader();
            if (clSocket.Available == 0)
                return;
            if (!ReceiveMessageHeader(hdr))
                return;
            IMessage msg;
            if (!ReceiveMessage(hdr, out msg))
                return;
            OnMessage(msg);
        }

        private void OnServerConnectionReset()
        {
            Root.Log(LogLevel.Info, "Client: connection lost");
            Reset();
        }

        private bool ReceiveMessageHeader(MsgHeader hdr)
        {
            try
            {
                hdr.Load(netStream, cfg.Protocol);
            }
            catch (MessageLoadException e)
            {
                if (NetUtil.CheckConnectionReset(e))
                    OnServerConnectionReset();
                else
                {
                    Root.Log(LogLevel.Warning, "Client: bad message header received from server ({0})", e.Message);
                    Disconnect();
                }
                return false;
            }
            return true;
        }

        private bool ReceiveMessage(MsgHeader hdr, out IMessage msg)
        {
            msg = null;
            // XXX: could be optimized - use one large buffer + unclosable MemoryStream
            var buf = new byte[hdr.Size];
            try
            {
                netStream.Read(buf, 0, buf.Length);
            }
            catch (IOException e)
            {
                if (NetUtil.CheckConnectionReset(e))
                {
                    OnServerConnectionReset();
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
                        container.Load(msWrap, cfg.Protocol);
                        symCp.Key = sessionKey;
                        msg = container.Extract(hdr.Id, symCp, cfg.Protocol);
                    }
                }
                else
                {
                    msg = MessageFactory.CreateMessage(hdr.Id);
                    try
                    {
                        msg.Load(msWrap, cfg.Protocol);
                    }
                    catch (MessageLoadException e)
                    {
                        Root.Log(LogLevel.Warning, "Client: bad message received from server ({0})", e.Message);
                        Disconnect();
                        return false;
                    }
                }
            }
            return true;
        }

        public override void OnMessage(IMessage msg)
        {
            switch (msg.Id)
            {
            case MessageId.SvPublicKeyChallenge:
                OnPublicKeyChallenge(msg as SvPublicKeyChallenge);
                break;
            case MessageId.SvAuthChallenge:
                OnAuthChallenge(msg as SvAuthChallenge);
                break;
            case MessageId.SvAuthResult:
                OnAuthResult(msg as SvAuthResult);
                break;
            case MessageId.SvDisconnect:
                OnDisconnect(msg as SvDisconnect);
                break;
            default:
                OnUserMessage(msg);
                break;
            }
        }

        private void OnPublicKeyChallenge(SvPublicKeyChallenge msg)
        {
            asymCp.KeySize = msg.KeySize;
            var respond = new ClPublicKeyRespond { Key = asymCp.GetKey() };
            Send(respond);
        }

        private void OnAuthChallenge(SvAuthChallenge msg)
        {
            sessionKey = asymCp.Decrypt(msg.SessionKey);
            var respond = new ClAuthRespond { Login = login, Password = password };
            Send(respond);
        }

        private void OnAuthResult(SvAuthResult msg)
        {
            if (msg.Result == Core.AuthResult.Accepted)
            {
                authenticated = true;
                Root.Log(LogLevel.Info, "Server: authentication succeeded <{0}>", msg.Message);
            }
            else
            {
                Root.Log(LogLevel.Error, "Server: authentication failed <{0}>", msg.Message);
                Reset(); // server closes connection after rejection
            }
            OnAuthResult(msg.Result, msg.Message);
        }

        private void OnDisconnect(SvDisconnect msg)
        {
            string info;
            switch (msg.Reason)
            {
            case DisconnectReason.Shutdown:
                info = "server shutdown";
                break;
            case DisconnectReason.Banned:
                info = "banned";
                break;
            default:
                info = "unknown";
                break;
            }
            var infoEx = msg.Message == "" ? "" : String.Format(" <{0}>", msg.Message);
            Root.Log(LogLevel.Info, "Client: disconnect received: {0}{1}", info, infoEx);
            disconnectReceived = true;
            Disconnect();
        }
        
        // XXX: write generalized version (both for server and client) and move to core
        public override void Send(IMessage msg)
        {
            if (!Connected)
            {
                Root.Log(LogLevel.Debug, "Client: attempt to send message with no connection");
                return;
            }
            using (var rawBuf = new MemoryStream())
            {
                var buf = rawBuf.AsUnclosable();
                var header = new MsgHeader();
                header.Id = msg.Id;
                header.Flags = MessageFlags.None;
                if (sessionKey != null)
                {
                    using (var container = new MessageCryptoContainer())
                    {
                        symCp.Key = sessionKey;
                        // XXX: generate new symCp.IV
                        container.Store(msg, symCp, cfg.Protocol);
                        container.Save(buf, cfg.Protocol);
                        header.Size = (int)buf.Length;
                        header.Flags |= MessageFlags.Secure;
                    }
                }
                else
                {
                    msg.Save(buf, cfg.Protocol);
                    header.Size = (int)buf.Length;
                }
                header.Save(netStream, cfg.Protocol);
                rawBuf.WriteTo(netStream);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    clSocket.Dispose();
                DisposeHelper.OnDispose<Client>(disposing);
                disposed = true;
            }
        }
    }
}
