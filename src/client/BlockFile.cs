using System;
using System.IO;
using Sdm.Core.Util;

namespace Sdm.Client
{
    internal sealed class BlockFileReader : IDisposable
    {
        private readonly FileStream fs;
        private long currentBlock;
        private bool disposed;

        public BlockFileReader(string filename, int blockSize, int bufferSize = 1024 * 1024)
        {
            var fi = new FileInfo(filename);
            Size = fi.Length;
            BlockSize = blockSize;
            BlockCount = Size / BlockSize;
            Padding = BlockSize - (int)(Size - BlockCount * BlockSize);
            if (Padding > 0)
                BlockCount++;
            fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
        }

        public bool Read(byte[] dst)
        {
            var size = BlockSize;
            if (currentBlock == BlockCount - 1)
            {
                size -= Padding;
                if (dst.Length != size)
                {
                    throw new ArgumentException("Destination buffer length must be equal to BlockSize - Padding" +
                        " (last block)");
                }
            }
            else
            {
                if (dst.Length != size)
                    throw new ArgumentException("Destination buffer length must be equal to BlockSize");
            }
            if (currentBlock >= BlockCount)
                return false;
            fs.Read(dst, 0, size);
            currentBlock++;
            return true;
        }

        public long Size { get; private set; }

        public int BlockSize { get; private set; }

        public long BlockCount { get; private set; }

        public long CurrentBlock 
        {
            get { return currentBlock; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Block index must be non-negative");
                if (value > BlockCount)
                    throw new ArgumentOutOfRangeException("Block index must be less than BlockCount");
                currentBlock = value;
                fs.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
            }
        }
        
        public int Padding { get; private set; }

        public bool Eof { get { return CurrentBlock >= BlockCount; } }

        public void Flush() { fs.Flush(); }

        public void Close() { Dispose(); }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true); 
            GC.SuppressFinalize(this);
        }

        ~BlockFileReader() { Dispose(false); }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    fs.Dispose();
                DisposeHelper.OnDispose<BlockFileReader>(disposing);
                disposed = true;
            }
        }

        #endregion
    }

    internal sealed class BlockFileWriter : IDisposable
    {
        private readonly FileStream fs;
        private long currentBlock;
        private bool disposed;

        public BlockFileWriter(string filename, int blockSize, long fileSize, int bufferSize = 128 * 1024)
        {
            Size = fileSize;
            BlockSize = blockSize;
            BlockCount = Size / BlockSize;
            Padding = BlockSize - (int)(Size - BlockCount * BlockSize);
            if (Padding > 0)
                BlockCount++;
            fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize);
            fs.SetLength(fileSize);
            fs.Flush();
        }

        public bool Write(byte[] src)
        {
            var size = BlockSize;
            if (currentBlock == BlockCount - 1)
            {
                size -= Padding;
                if (src.Length != size)
                {
                    throw new ArgumentException("Source buffer length must equal to BlockSize - Padding" +
                        " (last block)");
                }
            }
            else
            {
                if (src.Length != size)
                    throw new ArgumentException("Source buffer length must be equal to BlockSize");
            }
            if (currentBlock >= BlockCount)
                return false;
            fs.Write(src, 0, size);
            currentBlock++;
            return true;
        }

        public long Size { get; private set; }

        public int BlockSize { get; private set; }

        public long BlockCount { get; private set; }

        public long CurrentBlock
        {
            get { return currentBlock; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Block index must be non-negative");
                if (value > BlockCount)
                    throw new ArgumentOutOfRangeException("Block index must be less than BlockCount");
                currentBlock = value;
                fs.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
            }
        }

        public int Padding { get; private set; }

        public bool Eof { get { return CurrentBlock >= BlockCount; } }

        public void Flush() { fs.Flush(); }

        public void Close() { Dispose(); }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BlockFileWriter() { Dispose(false); }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    fs.Dispose();
                DisposeHelper.OnDispose<BlockFileWriter>(disposing);
                disposed = true;
            }
        }

        #endregion
    }
}
