using System;
using System.IO;

namespace Sdm.Core.Util
{
    public static class StreamUtil
    {
        public static int ReadTo(this Stream src, Stream dst, int count)
        {
            var buf = new byte[256];
            var readByteCount = 0;
            while (readByteCount < count)
            {
                var r = src.Read(buf, 0, Math.Min(buf.Length, count - readByteCount));
                if (r == 0)
                    break;
                dst.Write(buf, 0, r);
                readByteCount += r;
            }
            return readByteCount;
        }

        public static int ReadTo(this BinaryReader src, Stream dst, int count)
        {
            var buf = new byte[256];
            var readByteCount = 0;
            while (readByteCount < count)
            {
                var r = src.Read(buf, 0, Math.Min(buf.Length, count - readByteCount));
                if (r == 0)
                    break;
                dst.Write(buf, 0, r);
                readByteCount += r;
            }
            return readByteCount;
        }
    }
}
