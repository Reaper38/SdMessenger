using System;

namespace Sdm.Core
{
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
