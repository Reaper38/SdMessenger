using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Sdm.Core.Json;

namespace Sdm.Core.Messages
{
    // for messages without data
    public class DummyMessage<T> : MessageIdChecker<T>
    {
        protected DummyMessage(MessageId id) : base(id) {}
        public override void Load(Stream s, ProtocolId ptype) {}
        public override void Save(Stream s, ProtocolId ptype) {}
    }
    
    public abstract class MultiprotocolMessage<T> : MessageIdChecker<T>
    {
        protected MultiprotocolMessage(MessageId id) : base(id) {}

        // ISdmSerializable
        public override sealed void Load(Stream s, ProtocolId ptype)
        {
            try
            {
                switch (ptype)
                {
                case ProtocolId.Binary: LoadBin(s); return;
                case ProtocolId.Json: LoadJson(s); return;
                }
            }
            catch (MessageLoadException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new MessageLoadException(e.Message, e);
            }
            throw new NotSupportedException("Unsupported protocol");
        }

        public override sealed void Save(Stream s, ProtocolId ptype)
        {
            switch (ptype)
            {
            case ProtocolId.Binary: SaveBin(s); break;
            case ProtocolId.Json: SaveJson(s); break;
            default: throw new NotSupportedException("Unsupported protocol");
            }
        }
        // ~ISdmSerializable

        protected abstract void LoadJson(Stream s);
        protected abstract void SaveJson(Stream s);
        protected abstract void LoadBin(Stream s);
        protected abstract void SaveBin(Stream s);
    }

    public class SvPublicKeyChallenge : MultiprotocolMessage<SvPublicKeyChallenge>
    {
        public int KeySize;

        public SvPublicKeyChallenge() : base(MessageId.SvPublicKeyChallenge) {}
        
        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                KeySize = obj.GetInt32("keysz");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("keysz");
                w.WriteValue(KeySize);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                KeySize = r.ReadInt32();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(KeySize);
                w.Flush();
            }
        }
    }

    public class ClPublicKeyRespond : MultiprotocolMessage<ClPublicKeyRespond>
    {
        public string Key;

        public ClPublicKeyRespond() :
            base(MessageId.ClPublicKeyRespond)
        {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                Key = obj.GetString("key");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("key");
                w.WriteValue(Key);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                Key = r.ReadString();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(Key);
                w.Flush();
            }
        }
    }

    // encrypted session key
    public class SvAuthChallenge : MultiprotocolMessage<SvAuthChallenge>
    {
        /// <summary>Encrypted session key</summary>
        public byte[] SessionKey;

        public SvAuthChallenge() : base(MessageId.SvAuthChallenge) {}
        
        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var skey = obj.GetString("skey");
                try
                {
                    SessionKey = Convert.FromBase64String(skey);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid skey format");
                }
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                var skey = Convert.ToBase64String(SessionKey);
                w.WriteStartObject();
                w.WritePropertyName("skey");
                w.WriteValue(skey);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                var len = r.ReadInt32();
                SessionKey = r.ReadBytes(len);
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(SessionKey.Length);
                w.Write(SessionKey);
                w.Flush();
            }
        }
    }

    public class ClAuthRespond : MultiprotocolMessage<ClAuthRespond>
    {
        public string Login, Password;

        public ClAuthRespond() : base(MessageId.ClAuthRespond) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                Login = obj.GetString("login");
                Password = obj.GetString("password");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("login");
                w.WriteValue(Login);
                w.WritePropertyName("password");
                w.WriteValue(Password);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                Login = r.ReadString();
                Password = r.ReadString();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(Login);
                w.Write(Password);
                w.Flush();
            }
        }
    }

    public class SvAuthResult : MultiprotocolMessage<SvAuthResult>
    {
        public AuthResult Result;
        public string Message;

        public SvAuthResult() : base(MessageId.SvAuthResult) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var tmp = obj.GetInt32("result");
                try
                {
                    Result = (AuthResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid result: " + tmp);
                }
                Message = obj.GetString("msg");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("result");
                w.WriteValue((int)Result);
                w.WritePropertyName("msg");
                w.WriteValue(Message);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                var tmp = r.ReadByte();
                try
                {
                    Result = (AuthResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid result: " + tmp);
                }
                Message = r.ReadString();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write((byte)Result);
                w.Write(Message);
                w.Flush();
            }
        }
    }

    public class ClDisconnect : DummyMessage<ClDisconnect>
    {
        public ClDisconnect() : base(MessageId.ClDisconnect) {}
    }

    public class SvDisconnect : MultiprotocolMessage<SvDisconnect>
    {
        public DisconnectReason Reason;
        public string Message;

        public SvDisconnect() : base(MessageId.SvDisconnect) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var tmp = obj.GetInt32("reason");
                try
                {
                    Reason = (DisconnectReason)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid reason: " + tmp);
                }
                Message = obj.GetString("msg");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("result");
                w.WriteValue((int)Reason);
                w.WritePropertyName("msg");
                w.WriteValue(Message);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                var tmp = r.ReadByte();
                try
                {
                    Reason = (DisconnectReason)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid reason: " + tmp);
                }
                Message = r.ReadString();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write((byte)Reason);
                w.Write(Message);
                w.Flush();
            }
        }
    }
}
