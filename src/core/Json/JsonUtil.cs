using System.IO;
using Newtonsoft.Json;
using Sdm.Core.IO;

namespace Sdm.Core.Json
{
    public class JsonStreamWriter : JsonTextWriter
    {
        public JsonStreamWriter(Stream s)
            : base(new StreamWriter(s))
        {}
    }

    public class JsonStreamReader : JsonTextReader
    {
        public JsonStreamReader(Stream s, bool buffered = true)
            : base(buffered ? (TextReader)new StreamReader(s) : new UnbufferedStreamReader(s))
        { Unbuffered = !buffered; }
    }
}
