using System;
using Newtonsoft.Json.Linq;

namespace Sdm.Core.Messages
{
    public static class JsonHelper
    {
        private static JToken GetValue(JObject obj, string key, JTokenType expectedType)
        {
            JToken tok = obj[key];
            if (tok == null)
                throw new MessageLoadException("Key not found: " + key);
            if (tok.Type != expectedType)
            {
                var msg = String.Format("Token type mismatch: expected '{0}', got '{1}'", expectedType, tok.Type);
                throw new MessageLoadException(msg);
            }
            return tok;
        }

        public static int GetInt32(this JObject obj, string key)
        { return (int) GetValue(obj, key, JTokenType.Integer); }

        public static string GetString(this JObject obj, string key)
        { return (string)GetValue(obj, key, JTokenType.String); }
    }
}
