using System;
using Newtonsoft.Json.Linq;

namespace Sdm.Core.Messages
{
    public static class JsonHelper
    {
        public static int GetInt32(this JObject obj, string key)
        {
            var expectedType = JTokenType.Integer;
            JToken tok = obj[key];
            if (tok == null)
                throw new MessageLoadException("Key not found: " + key);
            if (tok.Type != expectedType)
            {
                var msg = String.Format("Token type mismatch: expected '{0}', got '{1}'", expectedType, tok.Type);
                throw new MessageLoadException(msg);
            }
            return (int)tok;
        }
    }
}
