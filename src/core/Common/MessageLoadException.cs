using System;

namespace Sdm.Core
{
    public class MessageLoadException : Exception
    {
        public MessageLoadException() {}

        public MessageLoadException(string msg) : base(msg) {}

        public MessageLoadException(string msg, Exception innerException) : base(msg, innerException) {}
    }
}
