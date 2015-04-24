using System;
using System.Net;
using System.Net.Sockets;
using Sdm.Core;
using Sdm.Core.Util;

namespace Sdm.Server
{
    internal sealed class ServerConfig
    {
        // [server]
        public ProtocolId Protocol = ProtocolId.Json;
        public IPAddress Address = IPAddress.Any;
        public ushort Port = 5477;
        // [security]
        public SdmSymmetricAlgorithm SymAlgorithm = SdmSymmetricAlgorithm.AES;
        public SdmAsymmetricAlgorithm AsymAlgorithm = SdmAsymmetricAlgorithm.RSA;
        public int SymAlgorithmKeySize = 256;
        public int AsymAlgorithmKeySize = 2048;
        // [socket]
        public bool UseIPv6 = false;
        public int SocketSendTimeout = 1000;
        public int SocketSendBufferSize = 0x8000;
        public int SocketReceiveBufferSize = 0x8000;
        public int SocketBacklog = 4;
        // [misc]
        public int UpdateSleep = 50;

        public ServerConfig() { Load(); }

        private void Load()
        {
            var cfg = SdmCore.Config;
            string tmp = null;
            if (cfg.ContainsSection("socket"))
            {
                cfg.TryGetBool("socket", "ipv6", ref UseIPv6);
                cfg.TryGetInt32("socket", "send_timeout", ref SocketSendTimeout);
                cfg.TryGetInt32("socket", "send_buffer_size", ref SocketSendBufferSize);
                cfg.TryGetInt32("socket", "recv_buffer_size", ref SocketReceiveBufferSize);
                cfg.TryGetInt32("socket", "backlog", ref SocketBacklog);
            }
            if (cfg.ContainsSection("server"))
            {
                if (cfg.TryGetString("server", "protocol", ref tmp))
                {
                    ProtocolId p;
                    if (Enum.TryParse(tmp, true, out p))
                        Protocol = p;
                }
                if (cfg.TryGetString("server", "address", ref tmp))
                {
                    var af = UseIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
                    try
                    {
                        Address = NetUtil.GetHostByName(tmp, af);
                    }
                    catch { }
                }
                int port = 0;
                if (cfg.TryGetInt32("server", "port", ref port))
                {
                    if (0 <= port && port <= ushort.MaxValue)
                        Port = (ushort)port;
                }
            }
            if (cfg.ContainsSection("security"))
            {
                if (cfg.TryGetString("security", "sym_algorithm", ref tmp))
                {
                    SdmSymmetricAlgorithm sa;
                    if (Enum.TryParse(tmp, true, out sa))
                        SymAlgorithm = sa;
                }
                if (cfg.TryGetString("security", "asym_algorithm", ref tmp))
                {
                    SdmAsymmetricAlgorithm aa;
                    if (Enum.TryParse(tmp, true, out aa))
                        AsymAlgorithm = aa;
                }
                cfg.TryGetInt32("security", "sym_key_size", ref SymAlgorithmKeySize);
                cfg.TryGetInt32("security", "asym_key_size", ref AsymAlgorithmKeySize);
            }
            if (cfg.ContainsSection("misc"))
            {
                cfg.TryGetInt32("misc", "update_sleep", ref UpdateSleep);
            }
        }
    }
}
