using System;
using System.Collections.Generic;
using Sdm.Core.Messages;

namespace Sdm.Core
{
    public static class MessageFactory
    {
        private static Dictionary<MessageId, Type> dict = new Dictionary<MessageId, Type>();
        private static Dictionary<Type, MessageId> rdict = new Dictionary<Type, MessageId>();
        private static object sync = new object();

        static MessageFactory()
        {
            Register(MessageId.SvPublicKeyChallenge, typeof(SvPublicKeyChallenge));
            Register(MessageId.ClPublicKeyRespond, typeof(ClPublicKeyRespond));
            Register(MessageId.SvAuthChallenge, typeof(SvAuthChallenge));
            Register(MessageId.ClAuthRespond, typeof(ClAuthRespond));
            Register(MessageId.SvAuthResult, typeof(SvAuthResult));
            Register(MessageId.ClDisconnect, typeof(ClDisconnect));
            Register(MessageId.SvDisconnect, typeof(SvDisconnect));
            Register(MessageId.ClUserlistRequest, typeof(ClUserlistRequest));
            Register(MessageId.SvUserlistRespond, typeof(SvUserlistRespond));
            Register(MessageId.SvUserlistUpdate, typeof(SvUserlistUpdate));
            Register(MessageId.CsChatMessage, typeof(CsChatMessage));
            Register(MessageId.ClFileTransferRequest, typeof(ClFileTransferRequest));
            Register(MessageId.CsFileTransferRespond, typeof(CsFileTransferRespond));
            Register(MessageId.SvFileTransferRequest, typeof(SvFileTransferRequest));
            Register(MessageId.CsFileTransferResult, typeof(CsFileTransferResult));
            Register(MessageId.CsBlockTransfer, typeof(CsBlockTransfer));
            // register all messages here
        }

        private static void Register(MessageId newMsgId, Type newType)
        {
            lock (sync)
            {
                if (dict.ContainsKey(newMsgId))
                {
                    var currentType = dict[newMsgId];
                    if (currentType != newType)
                    {
                        throw new Exception(String.Format("Can't associate {0}.{1} with type {2} " +
                            "because it is already associated with type {3}.",
                            typeof(MessageId).FullName, newMsgId, newType.FullName, currentType.FullName));
                    }
                }
                else
                    dict.Add(newMsgId, newType);
                if (rdict.ContainsKey(newType))
                {
                    var currentMsgId = rdict[newType];
                    if (currentMsgId != newMsgId)
                    {
                        throw new Exception(String.Format("Can't associate type {0} with {1}.{2} " +
                            "because it is already associated with {1}.{3}.",
                            newType.FullName, typeof(MessageId).FullName, newMsgId, currentMsgId));
                    }
                }
                else
                    rdict.Add(newType, newMsgId);
            }
        }
        
        public static IMessage CreateMessage(MessageId id)
        {
            Type type;
            if (!dict.TryGetValue(id, out type))
                throw new InvalidOperationException(String.Format("Type with id '{0}' is not registered", id));
            return (IMessage)Activator.CreateInstance(type);
        }
    }
}
