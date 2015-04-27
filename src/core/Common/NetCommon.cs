using System;
using System.Text;

namespace Sdm.Core
{
    public struct ClientId : IComparable<ClientId>
    {
        public static readonly ClientId Server = new ClientId(0);
        private static int lastId = 1;
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

        public override string ToString() { return Value.ToString(); }
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
        Rejected = 1,
    }

    public enum ConnectionState
    {
        Connected = 0,
        Waiting = 1,
        Disconnected = 2,
    }

    public enum AuthResult : byte
    {
        Accepted = 0,
        InvalidLogin = 1,
        Banned = 2,
    }

    public static class AuthResultUtil
    {
        public static string GetDescription(this AuthResult ar)
        {
            switch (ar)
            {
            case AuthResult.Accepted: return "Login successful";
            case AuthResult.InvalidLogin: return "Invalid login/password";
            case AuthResult.Banned: return "You have been banned";
            default: return "Unknown result";
            }
        }
    }

    public enum DisconnectReason : byte
    {
        Unknown = 0,
        Shutdown = 1,
        Banned = 2,
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

    public static class ClientAccessFlagsUtil
    {
        public static bool FromShortString(out ClientAccessFlags flags, string s)
        {
            // 0=none r=read w=write x=admin a=all
            flags = ClientAccessFlags.Default;
            foreach (char f in s)
            {
                switch (f)
                {
                case '0': continue;
                case 'r': flags |= ClientAccessFlags.Receive; continue;
                case 'w': flags |= ClientAccessFlags.Send; continue;
                case 'x': flags |= ClientAccessFlags.Admin; continue;
                case 'a': flags |= ClientAccessFlags.Max; continue;
                default: return false;
                }
            }
            return true;
        }
        
        public static string ToShortString(ClientAccessFlags flags)
        {
            if (flags == ClientAccessFlags.Default)
                return "0";
            if (flags == ClientAccessFlags.Max)
                return "a";
            var sb = new StringBuilder(16);
            if ((flags & ClientAccessFlags.Receive) == ClientAccessFlags.Receive)
                sb.Append('r');
            if ((flags & ClientAccessFlags.Send) == ClientAccessFlags.Send)
                sb.Append('w');
            if ((flags & ClientAccessFlags.Admin) == ClientAccessFlags.Admin)
                sb.Append('x');
            return sb.ToString();
        }
    }

    public class NetStats : INetStatistics
    {
        public long BytesSent { get; private set; }
        public long BytesSentPerSec { get; private set; }
        public long BytesReceived { get; private set; }
        public long BytesReceivedPerSec { get; private set; }

        public virtual void OnDataSent(long byteCount)
        { BytesSent += byteCount; }

        public virtual void OnDataReceived(long byteCount)
        { BytesReceived += byteCount; }

        public void Clear()
        {
            BytesSent = 0;
            BytesSentPerSec = 0;
            BytesReceived = 0;
            BytesReceivedPerSec = 0;
        }
    }
}
