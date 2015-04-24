using System;
using System.IO;
using System.Text;

namespace Sdm.Core.IO
{
    public class UnbufferedStreamReader : TextReader
    {
        private Stream stream;
        private BinaryReader r;
        private bool own;
        private bool hasLenPos = false;
        private bool disposed = false;

        public UnbufferedStreamReader(Stream stream, Encoding encoding, bool own)
        {
            this.stream = stream;
            this.own = own;
            r = new BinaryReader(stream, encoding);
            try
            {
                var l = stream.Length;
                var p = stream.Position;
                hasLenPos = true;
            }
            catch // ignore exception and leave hasLenPos unset
            {}
        }

        public UnbufferedStreamReader(Stream stream) :
            this(stream, Encoding.Default, true)
        { }

        public bool EndOfStream
        {
            get
            {
                if (disposed)
                    throw new ObjectDisposedException("stream");
                // todo: check if there's async task in progress (see StreamReader impl)
                return hasLenPos && stream.Position >= stream.Length;
            }
        }

        public override int Read() { return EndOfStream ? -1 : r.ReadChar(); }

        public override int Peek() { return r.PeekChar(); }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                if (own)
                    r.Close();
            }
            disposed = true;
        }

        ~UnbufferedStreamReader() { Dispose(false); }
    }
}
