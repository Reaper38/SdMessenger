using System;
using System.Collections.Generic;
using System.IO;

namespace Sdm.Core
{
    namespace Detail
    {
        internal static class MessageIdRegistry
        {
            private static Dictionary<MessageId, Type> dict = new Dictionary<MessageId, Type>();
            private static Dictionary<Type, MessageId> rdict = new Dictionary<Type, MessageId>();
            private static object sync = new object();

            public static void Register(MessageId newMsgId, Type newType)
            {
                lock (sync)
                {
                    if (dict.ContainsKey(newMsgId))
                    {
                        var currentType = dict[newMsgId];
                        if (currentType != newType)
                        {
                            throw new Exception(String.Format("Can't associate MessageId.{0} with type {1} " +
                                "because it is already associated with type {2}.",
                                newMsgId, newType.FullName, currentType.FullName));
                        }
                    }
                    else
                        dict.Add(newMsgId, newType);
                    if (rdict.ContainsKey(newType))
                    {
                        var currentMsgId = rdict[newType];
                        if (currentMsgId != newMsgId)
                        {
                            throw new Exception(String.Format("Can't associate type {0} with MessageId.{1} " +
                                "because it is already associated with MessageId.{2}.",
                                newType.FullName, newMsgId, currentMsgId));
                        }
                    }
                    else
                        rdict.Add(newType, newMsgId);
                }
            }
        }
    }

    public abstract class MessageIdChecker<TMsg> : IMessage
    {
        private static MessageId id;

        protected MessageIdChecker(MessageId msgId)
        {
            Detail.MessageIdRegistry.Register(msgId, GetType());
            id = msgId;
        }

        public MessageId Id { get { return id; } }
        public abstract void Load(Stream s, ProtocolId ptype);
        public abstract void Save(Stream s, ProtocolId ptype);
    }
}
