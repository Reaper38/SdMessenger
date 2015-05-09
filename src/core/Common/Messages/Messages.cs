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
    
    // XXX: unit tests are required for all messages below

    public class ClFileTransferRequest : MultiprotocolMessage
    {
        public string Username, FileName;
        public byte[] FileHash;
        public long FileSize;
        public int BlockSize;
        public ulong Token;

        public ClFileTransferRequest() : base(MessageId.ClFileTransferRequest) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                Username = obj.GetString("usr");
                FileName = obj.GetString("file_name");
                FileSize = obj.GetInt32("file_size");
                var hash = obj.GetString("file_hash");
                try
                {
                    FileHash = Convert.FromBase64String(hash);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid file_hash: " + hash);
                }
                BlockSize = obj.GetInt32("block_size");
                var token = obj.GetString("token");
                try
                {
                    Token = UInt64.Parse(token);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid token: " + token);
                }
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("usr");
                w.WriteValue(Username);
                w.WritePropertyName("file_name");
                w.WriteValue(FileName);
                w.WritePropertyName("file_size");
                w.WriteValue(FileSize);
                w.WritePropertyName("file_hash");
                w.WriteValue(Convert.ToBase64String(FileHash));
                w.WritePropertyName("block_size");
                w.WriteValue(BlockSize);
                w.WritePropertyName("token");
                w.WriteValue(Token.ToString());
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                Username = r.ReadString();
                FileName = r.ReadString();
                FileSize = r.ReadInt64();
                var len = r.ReadInt32();
                FileHash = r.ReadBytes(len);
                BlockSize = r.ReadInt32();
                Token = r.ReadUInt64();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(Username);
                w.Write(FileName);
                w.Write(FileSize);
                w.Write(FileHash.Length);
                w.Write(FileHash);
                w.Write(BlockSize);
                w.Write(Token);
                w.Flush();
            }
        }
    }

    public class SvFileTransferRequest : MultiprotocolMessage
    {
        public string Username, FileName;
        public byte[] FileHash;
        public long FileSize;
        public int BlockSize;
        public FileTransferId SessionId;
        
        public SvFileTransferRequest() : base(MessageId.SvFileTransferRequest) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                Username = obj.GetString("usr");
                FileName = obj.GetString("file_name");
                FileSize = obj.GetInt32("file_size");
                var hash = obj.GetString("file_hash");
                FileHash = Convert.FromBase64String(hash);
                BlockSize = obj.GetInt32("block_size");
                var sid = obj.GetString("sid");
                SessionId = FileTransferId.Parse(sid);
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("usr");
                w.WriteValue(Username);
                w.WritePropertyName("file_name");
                w.WriteValue(FileName);
                w.WritePropertyName("file_size");
                w.WriteValue(FileSize);
                w.WritePropertyName("file_hash");
                w.WriteValue(Convert.ToBase64String(FileHash));
                w.WritePropertyName("block_size");
                w.WriteValue(BlockSize);
                w.WritePropertyName("sid");
                w.WriteValue(SessionId.ToString());
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                Username = r.ReadString();
                FileName = r.ReadString();
                FileSize = r.ReadInt64();
                var len = r.ReadInt32();
                FileHash = r.ReadBytes(len);
                BlockSize = r.ReadInt32();
                var sid = r.ReadString();
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + sid);
                }
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(Username);
                w.Write(FileName);
                w.Write(FileSize);
                w.Write(FileHash.Length);
                w.Write(FileHash);
                w.Write(BlockSize);
                w.Write(SessionId.ToString());
                w.Flush();
            }
        }
    }
    // XXX: add DstFileName field
    public class ClFileTransferRespond : MultiprotocolMessage
    {
        public FileTrasferResult Result;
        public FileTransferId SessionId;
        public int BlockSize;

        public ClFileTransferRespond() : base(MessageId.ClFileTransferRespond) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var tmp = obj.GetInt32("result");
                try
                {
                    Result = (FileTrasferResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid result: " + tmp);
                }
                var sid = obj.GetString("sid");
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + tmp);
                }
                BlockSize = obj.GetInt32("block_size");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("result");
                w.WriteValue((int)Result);
                w.WritePropertyName("sid");
                w.WriteValue(SessionId.ToString());
                w.WritePropertyName("block_size");
                w.WriteValue(BlockSize);
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
                    Result = (FileTrasferResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid result: " + tmp);
                }
                var sid = r.ReadString();
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + sid);
                }
                BlockSize = r.ReadInt32();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write((byte)Result);
                w.Write(SessionId.ToString());
                w.Write(BlockSize);
                w.Flush();
            }
        }
    }
    
    public class SvFileTransferResult : MultiprotocolMessage
    {
        public FileTrasferResult Result;
        public FileTransferId SessionId;
        public ulong Token;
        public int BlockSize;

        public SvFileTransferResult() : base(MessageId.SvFileTransferResult) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var tmp = obj.GetInt32("result");
                try
                {
                    Result = (FileTrasferResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid result: " + tmp);
                }
                var sid = obj.GetString("sid");
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + tmp);
                }
                var token = obj.GetString("token");
                try
                {
                    Token = UInt64.Parse(token);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid token: " + token);
                }
                BlockSize = obj.GetInt32("block_size");
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("result");
                w.WriteValue((int)Result);
                w.WritePropertyName("sid");
                w.WriteValue(SessionId.ToString());
                w.WritePropertyName("token");
                w.WriteValue(Token.ToString());
                w.WritePropertyName("block_size");
                w.WriteValue(BlockSize);
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
                    Result = (FileTrasferResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid result: " + tmp);
                }
                var sid = r.ReadString();
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + sid);
                }
                Token = r.ReadUInt64();
                BlockSize = r.ReadInt32();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write((byte)Result);
                w.Write(SessionId.ToString());
                w.Write(Token);
                w.Write(BlockSize);
                w.Flush();
            }
        }
    }

    public class CsFileTransferData : MultiprotocolMessage
    {
        public FileTransferId SessionId;
        public byte[] Data;

        public CsFileTransferData() : base(MessageId.CsFileTransferData) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var sid = obj.GetString("sid");
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + sid);
                }
                var data = obj.GetString("data");
                try
                {
                    Data = Convert.FromBase64String(data);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid data format");
                }
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("sid");
                w.WriteValue(SessionId.ToString());
                w.WritePropertyName("data");
                w.WriteValue(Convert.ToBase64String(Data));
                w.WriteEndObject();
                w.Flush();
            }
        }

        protected override void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                var sid = r.ReadString();
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + sid);
                }
                var len = r.ReadInt32();
                Data = r.ReadBytes(len);
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(SessionId.ToString());
                w.Write(Data.Length);
                w.Write(Data);
                w.Flush();
            }
        }
    }

    public class CsFileTransferVerificationResult : MultiprotocolMessage
    {
        public FileTransferVerificationResult Result;
        public FileTransferId SessionId;

        public CsFileTransferVerificationResult() : base(MessageId.CsFileTransferVerificationResult) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var tmp = obj.GetInt32("result");
                try
                {
                    Result = (FileTransferVerificationResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid result: " + tmp);
                }
                var sid = obj.GetString("sid");
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + tmp);
                }
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("result");
                w.WriteValue((int)Result);
                w.WritePropertyName("sid");
                w.WriteValue(SessionId.ToString());
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
                    Result = (FileTransferVerificationResult)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid result: " + tmp);
                }
                var sid = r.ReadString();
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + sid);
                }
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write((byte)Result);
                w.Write(SessionId.ToString());
                w.Flush();
            }
        }
    }

    // 1] sender wants to interrupt <waiting> transfer (token is required)
    // 2] sender/receiver wants to interrupt <working/...> transfer (session id is required)
    // note: client always receives this message with valid session id
    public class CsFileTransferInterruption : MultiprotocolMessage
    {
        public FileTransferInterruption Int;
        public FileTransferId SessionId;
        public ulong Token;

        public CsFileTransferInterruption() : base(MessageId.CsFileTransferInterruption) {}

        protected override void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                var tmp = obj.GetInt32("int");
                try
                {
                    Int = (FileTransferInterruption)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid int: " + tmp);
                }
                var sid = obj.GetString("sid");
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + tmp);
                }
                var token = obj.GetString("token");
                try
                {
                    Token = UInt64.Parse(token);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid token: " + token);
                }
            }
        }

        protected override void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("int");
                w.WriteValue((int)Int);
                w.WritePropertyName("sid");
                w.WriteValue(SessionId.ToString());
                w.WritePropertyName("token");
                w.WriteValue(Token.ToString());
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
                    Int = (FileTransferInterruption)tmp;
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid int: " + tmp);
                }
                var sid = r.ReadString();
                try
                {
                    SessionId = FileTransferId.Parse(sid);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid sid: " + sid);
                }
                Token = r.ReadUInt64();
            }
        }

        protected override void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write((byte)Int);
                w.Write(SessionId.ToString());
                w.Write(Token);
                w.Flush();
            }
        }
    }
}
