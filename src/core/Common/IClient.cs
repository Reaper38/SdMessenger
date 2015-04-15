using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Sdm.Core
{
    // represents remote client (for server app)
    public interface IClient
    {
        ClientId Id { get; }
        IPAddress Address { get; }
        ushort Port { get; }
        INetStatistics Stats { get; }
        string Login { get; set; }
        string Password { get; set; }        
        ClientAccessFlags AccessFlags { get; set; }
        string SessionKey { get; } // AES key
        // client state could be added here

        Stream NetStream { get; }       
    }
}
