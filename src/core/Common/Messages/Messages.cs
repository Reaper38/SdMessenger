using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sdm.Core.Messages
{
    // for messages without data
    public class DummyMessage<T> : MessageIdChecker<T>
    {
        protected DummyMessage(MessageId id) : base(id) {}
        public override void Load(Stream s, ProtocolId ptype) {}
        public override void Save(Stream s, ProtocolId ptype) {}
    }
}
