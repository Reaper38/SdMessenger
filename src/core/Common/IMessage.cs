using System;
using System.IO;

namespace Sdm.Core
{
    public interface ISdmSerializable
    {
        // Stream must not be closed/disposed here
        void Load(Stream s, ProtocolId ptype);
        // Stream must not be closed/disposed here
        void Save(Stream s, ProtocolId ptype);
    }

    public interface IMessage : ISdmSerializable
    {
        MessageId Id { get; }
    }

    [Flags]
    public enum MessageFlags : byte
    {
        None = 0,
        Secure = 1,
    }

    public interface IMessageHeader : ISdmSerializable
    {
        int Size { get; }
        MessageFlags Flags { get; }
        MessageId Id { get; }
    }
}
