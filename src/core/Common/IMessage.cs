using System.IO;

namespace Sdm.Core
{
    public interface IMessage
    {
        MessageId Id { get; }
        // Stream must not be closed/disposed here
        void Load(Stream s, ProtocolId ptype);
        // Stream must not be closed/disposed here
        void Save(Stream s, ProtocolId ptype);
    }
}
