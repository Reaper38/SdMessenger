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
}
