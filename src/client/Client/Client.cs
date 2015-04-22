using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Sdm.Core;
using Sdm.Core.Messages;
using Sdm.Core.Util;

namespace Sdm.Client
{
    internal class Client : PureClientBase
    {
        private Socket clSocket;
        private NetworkStream rawNetStream;
        private Stream netStream; // unclosable
        private ConnectionState connectionState;
        private ClientId selfId; // XXX: remove
        private byte[] sessionKey; // null == no session key has been received yet
        private string login, password;
        private bool authenticated;
        private IAsymmetricCryptoProvider asymCp;
        private ISymmetricCryptoProvider symCp;
        private const ProtocolId Protocol = ProtocolId.Json;
        private bool disposed = false;

        public Client()
        {
            asymCp = CryptoProviderFactory.Instance.CreateAsymmetric(SdmAsymmetricAlgorithm.RSA);
            symCp = CryptoProviderFactory.Instance.CreateSymmetric(SdmSymmetricAlgorithm.AES);
        }

        private static void Log(LogLevel l, string msg)
        { SdmCore.Logger.Log(l, msg); }

        private static void Log(LogLevel l, string msg, params object[] args)
        { SdmCore.Logger.Log(l, String.Format(msg, args)); }

        public override ConnectionState ConnectionState
        { get { return connectionState; } }

        public override ClientId Id { get { return selfId; } }
        
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
            if (Connected)
                throw new InvalidOperationException("Already connected");
            this.login = login;
            this.password = password;
            connectionState = ConnectionState.Waiting;
            Log(LogLevel.Info, "Client: connecting to {0}:{1} ...", address, port);
            // XXX: get socket params from config
            clSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 1000,
                SendBufferSize = 0x8000,
                ReceiveBufferSize = 0x8000
            };
            rawNetStream = new NetworkStream(clSocket);
            netStream = rawNetStream.AsUnclosable();
            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(address, port);
            args.Completed += EndConnect;
            // XXX: handle exceptions thrown by ConnectAsync
            if (!clSocket.ConnectAsync(args))
                EndConnect(null, args);
        }

        private void Reset()
        {
            connectionState = ConnectionState.Disconnected;
            authenticated = false;
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
                Reset();
            }
            else
            {
                cr = Core.ConnectionResult.Accepted;
                connectionState = ConnectionState.Connected;
                msg = "Connection established";
            }
            OnConnectionResult(cr, msg);
        }

        public override void Disconnect()
        {
            var msg = new ClDisconnect();
            Send(msg);
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
            Reset();
        }

        private bool ReceiveMessageHeader(MsgHeader hdr)
        {
            try
            {
                hdr.Load(netStream, Protocol);
            }
            catch (MessageLoadException e)
            {
                if (NetUtil.CheckConnectionReset(e))
                    OnServerConnectionReset();
                else
                {
                    Log(LogLevel.Warning, "Client: bad message header received from server ({1})", e.Message);
                    // XXX: disconnect
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
                        container.Load(msWrap, Protocol);
                        symCp.Key = sessionKey;
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
                        Log(LogLevel.Warning, "Client: bad message received from server ({0})", e.Message);
                        // XXX: disconnect
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
            default:
                // log unknown message
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
                authenticated = true;
            else
                Reset(); // server closes connection after rejection
            OnAuthResult(msg.Result, msg.Message);
        }

        // XXX: write generalized version (both for server and client) and move to core
        public override void Send(IMessage msg)
        {
            if (!Connected)
            {
                Log(LogLevel.Debug, "Client: attempt to send message with no connection");
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
                        container.Store(msg, symCp, Protocol);
                        container.Save(buf, Protocol);
                        header.Size = (int)buf.Length;
                        header.Flags |= MessageFlags.Secure;
                    }
                }
                else
                {
                    msg.Save(buf, Protocol);
                    header.Size = (int)buf.Length;
                }
                header.Save(netStream, Protocol);
                rawBuf.WriteTo(netStream);
            }
        }

        private bool Connected
        { get { return ConnectionState == ConnectionState.Connected; } }

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
