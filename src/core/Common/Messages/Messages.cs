using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Sdm.Core.Json;

namespace Sdm.Core.Messages
{
    // for messages without data
    public class DummyMessage : IMessage
    {
        protected DummyMessage(MessageId id)
        { Id = id; }
        public void Load(Stream s, ProtocolId ptype) {}
        public void Save(Stream s, ProtocolId ptype) {}
        public MessageId Id { get; private set; }
        public virtual bool AuthRequired { get { return true; }}
    }
    
    public abstract class MultiprotocolMessage : IMessage
    {
        protected MultiprotocolMessage(MessageId id)
        { Id = id; }

        // ISdmSerializable
        public void Load(Stream s, ProtocolId ptype)
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

        public void Save(Stream s, ProtocolId ptype)
        {
            switch (ptype)
            {
            case ProtocolId.Binary: SaveBin(s); break;
            case ProtocolId.Json: SaveJson(s); break;
            default: throw new NotSupportedException("Unsupported protocol");
            }
        }
        // ~ISdmSerializable

        public MessageId Id { get; private set; }
        public virtual bool AuthRequired { get { return true; } }

        protected abstract void LoadJson(Stream s);
        protected abstract void SaveJson(Stream s);
        protected abstract void LoadBin(Stream s);
        protected abstract void SaveBin(Stream s);
    }

    public class SvPublicKeyChallenge : MultiprotocolMessage
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

    public class ClPublicKeyRespond : MultiprotocolMessage
    {
        public string Key;

        public ClPublicKeyRespond() : base(MessageId.ClPublicKeyRespond) {}

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
    public class SvAuthChallenge : MultiprotocolMessage
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

    public class ClAuthRespond : MultiprotocolMessage
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

    public class SvAuthResult : MultiprotocolMessage
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

    public class ClDisconnect : DummyMessage
    {
        public ClDisconnect() : base(MessageId.ClDisconnect) {}
    }

    public class SvDisconnect : MultiprotocolMessage
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
                w.WritePropertyName("reason");
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

    public class ClUserlistRequest : DummyMessage
    {
        public ClUserlistRequest() : base(MessageId.ClUserlistRequest) {}
    }

    public class SvUserlistRespond : MultiprotocolMessage
    {
        public string[] Usernames;

        public SvUserlistRespond() : base(MessageId.SvUserlistRespond) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                Usernames = obj.GetArray<string>("unames");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("unames");
                w.WriteStartArray();
                foreach (var uname in Usernames)
                    w.WriteValue(uname);
                w.WriteEndArray();
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                var len = r.ReadInt32();
                Usernames = new string[len];
                for (int i = 0; i < len; i++)
                    Usernames[i] = r.ReadString();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(Usernames.Length);
                foreach (var uname in Usernames)
                    w.Write(uname);
                w.Flush();
            }
        }
    }

    public class SvUserlistUpdate : MultiprotocolMessage
    {
        public string[] Connected = {};
        public string[] Disconnected = {};

        public SvUserlistUpdate() : base(MessageId.SvUserlistUpdate) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                Connected = obj.GetArray<string>("add");
                Disconnected = obj.GetArray<string>("del");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("add");
                w.WriteStartArray();
                foreach (var uname in Connected)
                    w.WriteValue(uname);
                w.WriteEndArray();
                w.WritePropertyName("del");
                w.WriteStartArray();
                foreach (var uname in Disconnected)
                    w.WriteValue(uname);
                w.WriteEndArray();
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                var len = r.ReadInt32();
                Connected = new string[len];
                for (int i = 0; i < len; i++)
                    Connected[i] = r.ReadString();
                len = r.ReadInt32();
                Disconnected = new string[len];
                for (int i = 0; i < len; i++)
                    Disconnected[i] = r.ReadString();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(Connected.Length);
                foreach (var uname in Connected)
                    w.Write(uname);
                w.Write(Disconnected.Length);
                foreach (var uname in Disconnected)
                    w.Write(uname);
                w.Flush();
            }
        }
    }

    public class CsChatMessage : MultiprotocolMessage
    {
        // XXX: add timestamp
        public string Username, Message;

        public CsChatMessage() : base(MessageId.CsChatMessage) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                Username = obj.GetString("usr");
                Message = obj.GetString("msg");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("usr");
                w.WriteValue(Username);
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
                Username = r.ReadString();
                Message = r.ReadString();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(Username);
                w.Write(Message);
                w.Flush();
            }
        }
    }

    public class ClFileTransferRequest : MultiprotocolMessage
    {
        public string Username, Filename, Hash;
        public int Size, Limit;
        public ClFileTransferRequest() : base(MessageId.ClFileTransferRequest) { }
        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                Username = obj.GetString("usr");
                Filename = obj.GetString("fnm");
                Size = obj.GetInt32("fsz");
                Hash = obj.GetString("fhs");
                Limit = obj.GetInt32("lim");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("usr");
                w.WriteValue(Username);
                w.WritePropertyName("fnm");
                w.WriteValue(Filename);
                w.WritePropertyName("fsz");
                w.WriteValue(Size.ToString());
                w.WritePropertyName("fhs");
                w.WriteValue(Hash);
                w.WritePropertyName("lim");
                w.WriteValue(Limit);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                Username = r.ReadString();
                Filename = r.ReadString();
                Size = r.ReadInt32();
                Hash = r.ReadString();
                Limit = r.ReadInt32();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(Username);
                w.Write(Filename);
                w.Write(Size);
                w.Write(Hash);
                w.Write(Limit);
                w.Flush();
            }
        }
    }

    public class SvFileTransferRequest : MultiprotocolMessage
    {
        public string Username, Filename, Hash, SessionId;
        public int Size;
        public SvFileTransferRequest() : base(MessageId.SvFileTransferRequest) { }
        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                SessionId = obj.GetString("sid");
                Username = obj.GetString("usr");
                Filename = obj.GetString("fnm");
                Size = Convert.ToInt32(obj.GetString("fsz"));
                Hash = obj.GetString("fhs");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("sid");
                w.WriteValue(SessionId);
                w.WritePropertyName("usr");
                w.WriteValue(Username);
                w.WritePropertyName("fnm");
                w.WriteValue(Filename);
                w.WritePropertyName("fsz");
                w.WriteValue(Size.ToString());
                w.WritePropertyName("fhs");
                w.WriteValue(Hash);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                SessionId = r.ReadString();
                Username = r.ReadString();
                Filename = r.ReadString();
                Size = r.ReadInt32();
                Hash = r.ReadString();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(SessionId);
                w.Write(Username);
                w.Write(Filename);
                w.Write(Size);
                w.Write(Hash);
                w.Flush();
            }
        }
    }

    public class CsFileTransferRespond : MultiprotocolMessage
    {
        public FileTrasferResult Flag;
        public string SessionId;
        public int Limit;
        public CsFileTransferRespond() : base(MessageId.CsFileTransferRespond) { }
        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var tmp = obj.GetInt32("flg");
                try
                {
                    Flag = (FileTrasferResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid flg: " + tmp);
                }
                Limit = obj.GetInt32("lim");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("sid");
                w.WriteValue(SessionId);
                w.WritePropertyName("flg");
                w.WriteValue((int)Flag);
                w.WritePropertyName("lim");
                w.WriteValue(Limit);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                SessionId = r.ReadString();
                var tmp = r.ReadByte();
                try
                {
                    Flag = (FileTrasferResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid flg: " + tmp);
                }
                Limit = r.ReadInt32();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(SessionId);
                w.Write((byte)Flag);
                w.Write(Limit);
                w.Flush();
            }
        }
    }

 

    public class CsFileTransferResult : MultiprotocolMessage
    {
        public FileTrasferResult Flag;
        public string SessionId;
        public CsFileTransferResult() : base(MessageId.CsFileTransferResult) { }

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                SessionId = obj.GetString("sid");
                var tmp = obj.GetInt32("result");
                try
                {
                    Flag = (FileTrasferResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid flg: " + tmp);
                }
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("sid");
                w.WriteValue(SessionId);
                w.WritePropertyName("flg");
                w.WriteValue((int)Flag);
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                SessionId = r.ReadString();
                var tmp = r.ReadByte();
                try
                {
                    Flag = (FileTrasferResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid flg: " + tmp);
                }
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(SessionId);
                w.Write(Flag.ToString());
                w.Flush();
            }
        }
    }

    public class CsBlockTransfer : MultiprotocolMessage
    {
        public byte[] Block;
        public string SessionId;

        public CsBlockTransfer() : base(MessageId.CsBlockTransfer) { }
        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                SessionId = obj.GetString("sid");
                var blk = obj.GetString("blk");
                try
                {
                    Block = Convert.FromBase64String(blk);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid block format");
                }             
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WritePropertyName("sid");
                w.WriteValue(SessionId);
                w.WritePropertyName("blk");
                w.WriteValue(Convert.ToBase64String(Block));
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                SessionId = r.ReadString();
                var len = r.ReadInt32();
                Block = r.ReadBytes(len);
            }
        }

        protected override void SaveBin(Stream s)
        {

            using (var w = new BinaryWriter(s))
            {
                w.Write(SessionId);
                w.Write(Block.Length);
                w.Write(Block);
                w.Flush();
            }
        }
    }
}
