using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Sdm.Core.Json;
using Sdm.Core.Util;

namespace Sdm.Core.Messages
{
    public sealed class MessageCryptoContainer : ISdmSerializable, IDisposable
    {
        private byte[] iv;
        private readonly MemoryStream cdata;
        private readonly Stream cdataWrap; // unclosable wrapper over cdata
        private bool disposed = false;

        public MessageCryptoContainer()
        {
            cdata = new MemoryStream(1024);
            cdata.SetLength(0);
            cdataWrap = cdata.AsUnclosable();
        }

        public void Store(IMessage msg, ISymmetricCryptoProvider cryptoProvider, ProtocolId protocol)
        {
            using (var src = new MemoryStream())
            {
                var srcWrap = src.AsUnclosable();
                msg.Save(srcWrap, protocol);
                srcWrap.Seek(0, SeekOrigin.Begin);
                cdataWrap.Seek(0, SeekOrigin.Begin);
                cryptoProvider.Encrypt(cdataWrap, srcWrap, (int)src.Length);
                iv = cryptoProvider.IV;
            }
        }

        public IMessage Extract(MessageId id, ISymmetricCryptoProvider cryptoProvider, ProtocolId protocol)
        {
            using (var dst = new MemoryStream())
            {
                var dstWrap = dst.AsUnclosable();
                cdataWrap.Seek(0, SeekOrigin.Begin);
                cryptoProvider.IV = iv;
                cryptoProvider.Decrypt(dstWrap, cdataWrap, (int)cdata.Length);
                dstWrap.Seek(0, SeekOrigin.Begin);
                var msg = MessageFactory.CreateMessage(id);
                msg.Load(dstWrap, protocol);
                return msg;
            }
        }

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
                throw new MessageLoadException(e.Message);
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

        private void LoadJson(Stream s)
        {
            using (var r = new JsonStreamReader(s))
            {
                var obj = JObject.Load(r);
                byte[] tmp;
                var siv = obj.GetString("iv");
                try
                {
                    tmp = Convert.FromBase64String(siv);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid iv format");
                }
                iv = tmp;
                var scdata = obj.GetString("cdata");
                try
                {
                    tmp = Convert.FromBase64String(scdata);
                }
                catch (FormatException)
                {
                    throw new MessageLoadException("Invalid cdata format");
                }
                cdata.Seek(0, SeekOrigin.Begin);
                cdata.SetLength(0);
                cdata.Write(tmp, 0, tmp.Length);
            }
        }

        private void SaveJson(Stream s)
        {
            using (var w = new JsonStreamWriter(s))
            {
                var siv = Convert.ToBase64String(iv, 0, iv.Length);
                var scdata = Convert.ToBase64String(cdata.GetBuffer(), 0, (int)cdata.Length);
                w.WriteStartObject();
                w.WritePropertyName("iv");
                w.WriteValue(siv);
                w.WritePropertyName("cdata");
                w.WriteValue(scdata);
                w.WriteEndObject();
                w.Flush();
            }
        }

        private void LoadBin(Stream s)
        {
            using (var r = new BinaryReader(s))
            {
                var len = r.ReadInt32();
                iv = r.ReadBytes(len);
                len = r.ReadInt32();
                var read = r.ReadTo(cdata, len);
                if (len != read)
                    throw new MessageLoadException("Incomplete cdata");
            }
        }

        private void SaveBin(Stream s)
        {
            using (var w = new BinaryWriter(s))
            {
                w.Write(iv.Length);
                w.Write(iv);
                w.Write(cdata.Length);
                w.Write(cdata.GetBuffer(), 0, (int)cdata.Length);
                w.Flush();
            }
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
                    cdata.Dispose();
                DisposeHelper.OnDispose<MessageCryptoContainer>(disposing);
                disposed = true;
            }
        }

        ~MessageCryptoContainer() { Dispose(false); }

        #endregion
    }
}
