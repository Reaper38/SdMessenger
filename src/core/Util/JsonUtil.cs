using System.IO;
using Newtonsoft.Json;

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
        public JsonStreamReader(Stream s)
            : base(new StreamReader(s))
        {}
    }
}
