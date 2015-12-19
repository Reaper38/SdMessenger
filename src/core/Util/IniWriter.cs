using System;
using System.IO;
using System.Text;

namespace Sdm.Core.Util
{
    public sealed class IniWriter : IDisposable
    {
        private StreamWriter writer;
        private bool disposed = false;

        public IniWriter(string fileName, bool append, Encoding encoding)
        {
            writer = new StreamWriter(fileName, append, encoding);
        }

        public void WriteSection(string sectionName)
        {
            writer.Write("[");
            writer.Write(sectionName);
            writer.WriteLine("]");
        }

        public void Write(string key, string value)
        {
            WriteKey(key);
            writer.WriteLine(value);
        }

        public void Write(string key, int value)
        {
            WriteKey(key);
            writer.WriteLine(value);
        }

        public void Write(string key, bool value)
        {
            WriteKey(key);
            writer.WriteLine(value ? "1" : "0");
        }

        private void WriteKey(string key)
        {
            writer.Write(key);
            writer.Write(" = ");
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    writer.Dispose();
                DisposeHelper.OnDispose<IniWriter>(disposing);
                disposed = true;
            }
        }

        ~IniWriter() { Dispose(false); }

        #endregion
    }
}
