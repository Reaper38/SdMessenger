using System;
using System.Collections.Generic;
using Sdm.Core;

namespace Sdm.Server
{
    internal enum FileTransferState
    {
        Waiting,   // request from CL1 received, waiting for respond from CL2
        Working,   // ...
        Verification, // all blocks transferred, waiting for verification result from CL2
        Success,   // all blocks transferred, verification succeeded
        Failure,   // verification failed or client disconnected
        Cancelled, // CL1/CL2 cancelled transfer
    }

    internal class FileTransferSession
    {
        public readonly FileTransferId Id;
        public readonly ulong Token;
        public readonly string Sender;
        public readonly string Receiver;
        public FileTransferState State;
        public string SrcName; // full path on sender's machine
        public string DstName; // full path on receiver's machine
        public readonly byte[] Hash;
        public readonly long Size;
        public int BlockSize;
        public long BlocksDone;

        public long BlocksTotal
        {
            get
            {
                long count = Size / BlockSize;
                var padding = (int)(Size - count * BlockSize);
                if (padding > 0)
                    count++;
                return count;
            }
        }

        public FileTransferSession(FileTransferId id, ulong token, string sender, string receiver,
            byte[] hash, long size)
        {
            Id = id;
            Token = token;
            Sender = sender;
            Receiver = receiver;
            Hash = hash;
            Size = size;
        }
    }

    internal class FileTransferSessionContainer
    {
        private readonly Dictionary<FileTransferId, FileTransferSession> idToSession;
        private readonly Dictionary<string, HashSet<FileTransferId>> nameToId;
        private uint lastSid = 0;

        public FileTransferSessionContainer()
        {
            idToSession = new Dictionary<FileTransferId, FileTransferSession>();
            nameToId = new Dictionary<string, HashSet<FileTransferId>>();
        }

        public int SessionCount
        { get { return idToSession.Count; } }

        public FileTransferSession CreateSession(ulong token, string sender, string receiver,
            byte[] hash, long size)
        {
            var id = new FileTransferId(++lastSid);
            var ft = new FileTransferSession(id, token, sender, receiver, hash, size);
            idToSession.Add(id, ft);
            HashSet<FileTransferId> senderIds, receiverIds;
            if (!nameToId.TryGetValue(sender, out senderIds))
            {
                senderIds = new HashSet<FileTransferId>();
                nameToId.Add(sender, senderIds);
            }
            senderIds.Add(id);
            if (!nameToId.TryGetValue(receiver, out receiverIds))
            {
                receiverIds = new HashSet<FileTransferId>();
                nameToId.Add(receiver, receiverIds);
            }
            receiverIds.Add(id);
            return ft;
        }

        public void DeleteSession(FileTransferId id)
        {
            FileTransferSession ft;
            if (!idToSession.TryGetValue(id, out ft))
                return;
            HashSet<FileTransferId> senderIds, receiverIds;
            if (nameToId.TryGetValue(ft.Sender, out senderIds))
            {
                senderIds.Remove(id);
                if (senderIds.Count == 0)
                    nameToId.Remove(ft.Sender);
            }
            if (nameToId.TryGetValue(ft.Receiver, out receiverIds))
            {
                receiverIds.Remove(id);
                if (receiverIds.Count == 0)
                    nameToId.Remove(ft.Receiver);
            }
        }

        public IEnumerable<FileTransferId> GetUserSessions(string username)
        {
            HashSet<FileTransferId> ids;
            return nameToId.TryGetValue(username, out ids) ? ids : null;
        }

        public FileTransferSession GetSessionById(FileTransferId id)
        {
            FileTransferSession ft;
            return idToSession.TryGetValue(id, out ft) ? ft : null;
        }
    }
}
