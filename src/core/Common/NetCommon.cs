using System;

namespace Sdm.Core
{
    public struct ClientId
    {
        public int Value;
    }

    public interface INetStatistics
    {
        long BytesSent { get; }
        long BytesSentPerSec { get; }
        long BytesReceived { get; }
        long BytesReceivedPerSec { get; }
    }

    public enum ConnectionResult
    {
        Accepted = 0,
        InvalidHost = 1,
        InvalidLogin = 2,
        Rejected = 3,
    }

    [Flags]
    public enum ClientAccessFlags : uint
    {
        Default = 0, // can't communicate with other clients
        Send = 1, // can send messages to clients
        Receive = 2, // can receive messages from clients
        Admin = 4, // can manage server (change settings, disconnect players, etc)
        Max = 0xffffffff
    }
}
