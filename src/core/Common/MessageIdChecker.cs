using System.IO;

namespace Sdm.Core
{
    public abstract class MessageIdChecker<TMsg> : IMessage
    {
        private static MessageId id;

        protected MessageIdChecker(MessageId msgId)
        {
            MessageFactory.Register(msgId, GetType());
            id = msgId;
        }

        public MessageId Id { get { return id; } }
        public abstract void Load(Stream s, ProtocolId ptype);
        public abstract void Save(Stream s, ProtocolId ptype);
    }
}
