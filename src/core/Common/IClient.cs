using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Sdm.Core
{
    [Flags]
    public enum ClientFlags
    {
        None = 0,
        Secure = 1,
        Authenticated = 2,
    }
    // represents remote client (for server app)
    public interface IClient
    {
        ClientId Id { get; }
        IPAddress Address { get; }
        ushort Port { get; }
        INetStatistics Stats { get; }
        string Login { get; set; }
        string Password { get; set; }
        ClientFlags Flags { get; }
        ClientAccessFlags AccessFlags { get; set; }
        byte[] SessionKey { get; } // AES key
        // client state could be added here

        Stream NetStream { get; }
    }
}
