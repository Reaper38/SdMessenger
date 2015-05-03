using System.IO;
using System.Net.Sockets;

namespace Sdm.Core
{
    /// <summary>
    /// Represents a performance counting wrapper for NetworkStream.
    /// Close() and Dispose() are ignored. To close underlying stream use BaseStream.Close().
    /// </summary>
    public sealed class DiagNetStream : Stream
    {
        public NetStats Stats { get; private set; }
        private NetworkStream ns;

        public DiagNetStream(NetworkStream plainStream, NetStats stats)
        {
            ns = plainStream;
            Stats = stats;
        }

        public NetworkStream BaseStream { get { return ns; } }

        public override bool CanRead
        { get { return ns.CanRead; } }

        public override bool CanSeek
        { get { return ns.CanSeek; } }

        public override bool CanWrite
        { get { return ns.CanWrite; } }

        public override void Flush()
        { ns.Flush(); }

        public override long Length
        { get { return ns.Length; } }

        public override long Position
        {
            get { return ns.Position; }
            set { ns.Position = value; }
        }

        public override bool CanTimeout
        { get { return ns.CanTimeout; } }

        public override int ReadTimeout
        {
            get { return ns.ReadTimeout; }
            set { ns.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return ns.WriteTimeout; }
            set { ns.WriteTimeout = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = ns.Read(buffer, offset, count);
            Stats.OnDataReceived(read);
            return read;
        }
        
        public override int ReadByte()
        {
            var read = ns.ReadByte();
            Stats.OnDataReceived(read);
            return read;
        }

        public override void WriteByte(byte value)
        {
            ns.WriteByte(value);
            Stats.OnDataSent(1);
        }

        public override long Seek(long offset, SeekOrigin origin)
        { return ns.Seek(offset, origin); }

        public override void SetLength(long value)
        { ns.SetLength(value); }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ns.Write(buffer, offset, count);
            Stats.OnDataSent(count);
        }
    }
}
