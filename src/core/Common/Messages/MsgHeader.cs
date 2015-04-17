using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Sdm.Core.Json;

namespace Sdm.Core.Messages
{
    public class MsgHeader : IMessageHeader
    {
        // IMessageHeader
        public int Size { get; set; }
        public MessageFlags Flags { get; set; }
        public MessageId Id { get; set; }
        // ~IMessageHeader

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

        private static int MakeFourCC(char c0, char c1, char c2, char c3)
        { return (int)(byte)(c0) | ((int)(byte)(c1) << 8) | ((int)(byte)(c2) << 16) | ((int)(byte)(c3) << 24); }

        private static readonly int binHdrMagic = MakeFourCC('s', 'd', 'm', '>');

        private void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(binHdrMagic);
                w.Write((ushort)Size);
                w.Write((byte)0);
                w.Write((byte)Flags);
                w.Write((ushort)Id);
            }
        }

        private void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                var magic = r.ReadInt32();
                if (magic != binHdrMagic)
                    throw new MessageLoadException("Binary header magic value mismatch");
                Size = r.ReadUInt16();
                r.ReadByte();
                var tmpU8 = r.ReadByte();
                try
                {
                    Flags = (MessageFlags)tmpU8;
                }
                catch (InvalidCastException)
                {
                    throw new MessageLoadException("Invalid message flags: " + tmpU8);
                }
                var tmpU16 = r.ReadUInt16();
                try
                {
                    Id = (MessageId)tmpU16;
                }
                catch (InvalidCastException)
                {
                    throw new MessageLoadException("Invalid message id: " + tmpU16);
                }
            }
        }

        private void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                w.WriteStartObject();
                w.WritePropertyName("msz");
                w.WriteValue(Size);
                w.WritePropertyName("mid");
                w.WriteValue(Id.ToString());
                w.WritePropertyName("mflags");
                w.WriteValue((int)Flags);
                w.WriteEndObject();
                w.Flush();
            }
        }
        
        private void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s, false) { SupportMultipleContent = true })
            {
                var obj = JObject.Load(r);
                Size = obj.GetInt32("msz");
                int tmp = obj.GetInt32("mid");
                try
                {
                    Id = (MessageId)tmp;
                }
                catch (InvalidCastException)
                {
                    throw new MessageLoadException("Invalid message id: " + tmp);
                }
                tmp = obj.GetInt32("mflags");
                try
                {
                    Flags = (MessageFlags)tmp;
                }
                catch (InvalidCastException)
                {
                    throw new MessageLoadException("Invalid message flags: " + tmp);
                }
            }
        }
    }
}
