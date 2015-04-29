using System;
using System.IO;
using Sdm.Core.Util;

namespace Sdm.Client
{
    internal sealed class BlockFileReader : IDisposable
    {
        private readonly FileStream fs;
        private readonly BufferedStream bfs;
        private long currentBlock;
        private bool disposed;

        public BlockFileReader(string filename, int blockSize, int bufferSize)
        {
            var fi = new FileInfo(filename);
            Size = fi.Length;
            BlockSize = blockSize;
            BlockCount = Size / BlockSize;
            Padding = (int)(Size - BlockCount * BlockSize);
            if (Padding > 0)
                BlockCount++;
            fs = fi.OpenRead();
            bfs = new BufferedStream(fs, bufferSize);
        }

        public bool Read(byte[] dst)
        {
            if (dst.Length != BlockSize)
                throw new ArgumentException("Destination buffer length must be equal to BlockSize");
            if (currentBlock >= BlockCount)
                return false;
            bfs.Read(dst, 0, BlockSize);
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
                bfs.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
            }
        }
        
        public int Padding { get; private set; }

        public bool Eof { get { return CurrentBlock >= BlockCount; } }

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
                {
                    bfs.Dispose();
                    fs.Dispose();
                }
                DisposeHelper.OnDispose<BlockFileReader>(disposing);
                disposed = true;
            }
        }

        #endregion
    }

    internal sealed class BlockFileWriter : IDisposable
    {
        private readonly FileStream fs;
        private readonly BufferedStream bfs;
        private long currentBlock;
        private bool disposed;

        public BlockFileWriter(string filename, int blockSize, int bufferSize, long fileSize)
        {
            Size = fileSize;
            BlockSize = blockSize;
            BlockCount = Size / BlockSize;
            Padding = (int)(Size - BlockCount * BlockSize);
            if (Padding > 0)
                BlockCount++;
            fs = new FileStream(filename, FileMode.OpenOrCreate);
            fs.SetLength(fileSize);
            fs.Flush();
            bfs = new BufferedStream(fs, bufferSize);
        }

        public bool Write(byte[] src)
        {
            if (src.Length != BlockSize)
                throw new ArgumentException("Source buffer length must be equal to BlockSize");
            if (currentBlock >= BlockCount)
                return false;
            bfs.Write(src, 0, BlockSize);
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
                bfs.Seek(currentBlock * BlockSize, SeekOrigin.Begin);
            }
        }

        public int Padding { get; private set; }

        public bool Eof { get { return CurrentBlock >= BlockCount; } }

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
                {
                    bfs.Dispose();
                    fs.Dispose();
                }
                DisposeHelper.OnDispose<BlockFileWriter>(disposing);
                disposed = true;
            }
        }

        #endregion
    }
}
