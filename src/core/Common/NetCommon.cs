using System;

namespace Sdm.Core
{
    public struct ClientId : IComparable<ClientId>
    {
        private static int lastId;
        public readonly int Value;

        public ClientId(IClientParams clParams)
        {
            // XXX: improve id generation
            Value = lastId++;
        }

        public ClientId(int value)
        {
            Value = value;
        }
        
        public static bool operator ==(ClientId a, ClientId b)
        { return a.Value == b.Value; }

        public static bool operator !=(ClientId a, ClientId b)
        { return !(a == b); }

        public int CompareTo(ClientId other)
        { return Value.CompareTo(other.Value); }

        public bool Equals(ClientId other)
        { return Value == other.Value; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is ClientId && Equals((ClientId)obj);
        }

        public override int GetHashCode() { return Value.GetHashCode(); }
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
