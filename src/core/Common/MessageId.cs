using System;

namespace Sdm.Core
{
    // Cl : client to server
    // Sv : server to client
    // Cs : both directions
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
        SvUserlistUpdate = 9,
        CsChatMessage = 10,
        ClFileTransferRequest = 11,
        ClFileTransferRespond = 12,
        SvFileTransferRequest = 13,
        SvFileTransferResult = 14,
        CsFileTransferData = 15,
        CsFileTransferVerificationResult = 16,
        CsFileTransferInterruption = 17,
        Max // add new ids above
    }

    public static class MessageIdProps
    {
        private static readonly bool[] flags;
        private static readonly UserAccess[] reqAccess;

        static MessageIdProps()
        {
            const int idCount = (int)MessageId.Max;
            flags = new bool[idCount];
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
            // set required access
            reqAccess = new UserAccess[idCount];
        }

        public static bool IsAuthRequired(this MessageId msgId)
        { return flags[(int)msgId]; }

        public static UserAccess GetRequiredAccess(this MessageId msgId)
        { return reqAccess[(int)msgId]; }
    }
}
