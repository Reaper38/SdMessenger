﻿using System;
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
        bool Secure { get; }
        bool Authenticated { get; }
        bool DeferredDisconnect { get; }
        ClientAccessFlags AccessFlags { get; set; }
        byte[] SessionKey { get; } // AES key
        // client state could be added here
        Stream NetStream { get; }
    }
}
