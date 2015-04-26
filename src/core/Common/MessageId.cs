using System;

namespace Sdm.Core
{
    // Cl : client to server
    // Sv : server to client
    public enum MessageId : ushort
    {
        SvPublicKeyChallenge = 0,
        ClPublicKeyRespond = 1,
        SvAuthChallenge = 2,
        ClAuthRespond = 3,
        SvAuthResult = 4,
        ClDisconnect = 5,
        SvDisconnect = 6,
        ClUserlistRequest = 7,
        SvUserlistRespond = 8,
        SvClientDisconnected = 9,
        Max // add new ids above
    }

    public static class MessageIdProps
    {
        private static readonly bool[] flags;

        static MessageIdProps()
        {
            flags = new bool[(int)MessageId.Max];
            // auth is required by default
            for (int i = 0; i < flags.Length; i++)
                flags[i] = true;
            Action<MessageId, bool> setFlag = (id, val) => flags[(int)id] = val;
            // exclude auth/connection related messages
            setFlag(MessageId.SvPublicKeyChallenge, false);
            setFlag(MessageId.ClPublicKeyRespond, false);
            setFlag(MessageId.SvAuthChallenge, false);
            setFlag(MessageId.ClAuthRespond, false);
            setFlag(MessageId.SvAuthResult, false);
            setFlag(MessageId.ClDisconnect, false);
            setFlag(MessageId.SvDisconnect, false);
        }

        public static bool IsAuthRequired(this MessageId msgId)
        { return flags[(int)msgId]; }
    }
}
