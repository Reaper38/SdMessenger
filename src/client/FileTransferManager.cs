using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Sdm.Client.Controls;
using Sdm.Core;
using Sdm.Core.Messages;
using Sdm.Core.Util;

namespace Sdm.Client
{
    internal interface IFileTransfer : IDisposable
    {
        FileTransferDirection Direction { get; }
        FileTransferId Id { get; }
        FileTransferState State { get; }
        string ErrorMessage { get; }
        string Name { get; }
        byte[] Hash { get; }
        long BytesTotal { get; }
        long BytesDone { get; }
        int BlockSize { get; }
        long BlocksDone { get; }
        void Cancel();
        void Delete();
    }

    internal interface IOutcomingFileTransfer : IFileTransfer
    {
        string Receiver { get; }
        ulong Token { get; }
    }

    internal interface IIncomingFileTransfer : IFileTransfer
    {
        string Sender { get; }
        void Accept(string localName);
    }

    internal delegate void FileTransferStateHandler(IFileTransfer ft);

    internal delegate void FileTransferRequestHandler(IIncomingFileTransfer ft);

    internal delegate void FileTransferDataSentHandler(IOutcomingFileTransfer ft);

    internal delegate void FileTransferDataReceivedHandler(IIncomingFileTransfer ft);

    internal class FileTransferManager
    {
        private abstract class FileTransferBase : IFileTransfer
        {
            private readonly int hashCode;
            protected readonly FileTransferManager Owner;

            protected FileTransferBase(FileTransferManager owner)
            {
                // all file transfer instances are unique => assign random number to each instance as hash
                hashCode = new Random().Next();
                Owner = owner;
            }

            public abstract FileTransferDirection Direction { get; }
            public abstract FileTransferId Id { get; }
            public FileTransferState State { get; set; }
            public string ErrorMessage { get; set; }
            public string Name { get; set; }
            public byte[] Hash { get; set; }
            public long BytesTotal { get; set; }
            public long BytesDone { get; set; }
            public int BlockSize { get; set; }
            public long BlocksDone { get; set; }
            public abstract void Cancel();
            public abstract void Delete();
            protected abstract void Dispose(bool disposing);

            public override int GetHashCode() { return hashCode; }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~FileTransferBase() { Dispose(false); }
        }

        private class IncomingFileTransferInfo
        {
            public FileTransferId Id;
            public string Sender;
            public string Name;
            public long Size;
            public byte[] Hash;
            public int InitBlockSize;
        }

        private sealed class IncomingFileTransfer : FileTransferBase, IIncomingFileTransfer
        {
            private bool accepted = false;
            private bool deleted = false;
            private bool disposed = false;
            private FileTransferId id;
            public override FileTransferDirection Direction { get { return FileTransferDirection.In; } }
            public override FileTransferId Id { get { return id; } }
            public void SetId(FileTransferId newId) { id = newId; }
            public string Sender { get; private set; }
            public BlockFileWriter Writer;

            public IncomingFileTransfer(FileTransferManager owner, IncomingFileTransferInfo info) : base(owner)
            {
                id = info.Id;
                State = FileTransferState.Waiting;
                Sender = info.Sender;
                Name = info.Name;
                BytesTotal = info.Size;
                Hash = info.Hash;
                BlockSize = info.InitBlockSize;
            }

            // XXX: write to temp file 'filename.ext.tmp' and rename it to original name once transfer succeeded
            // XXX: delete file on transfer cancellation

            public override void Cancel()
            {
                IMessage msg;
                switch (State)
                {
                case FileTransferState.Success:
                case FileTransferState.Failure:
                case FileTransferState.Cancelled:
                    return;
                case FileTransferState.Waiting:
                    msg = new ClFileTransferRespond
                    {
                        Result = FileTransferRequestResult.Rejected,
                        SessionId = Id
                    };
                    break;
                case FileTransferState.Working:
                default:
                    msg = new CsFileTransferInterruption
                    {
                        SessionId = Id,
                        Int = FileTransferInterruption.Cancel
                    };
                    break;
                }
                State = FileTransferState.Cancelled;
                Delete();
                Owner.client.Send(msg);
                Owner.OnTransferStateChanged(this);
            }

            public override void Delete()
            {
                if (deleted)
                    return;
                deleted = true;
                Owner.delAssignedFts.Enqueue(Id);
            }

            public void Accept(string localName)
            {
                if (accepted)
                    return;
                accepted = true;
                Name = localName;
                State = FileTransferState.Working;
                Owner.OnTransferStateChanged(this);
            }
            
            protected override void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        if (Writer != null)
                        {
                            Writer.Dispose();
                            Writer = null;
                        }
                    }
                    DisposeHelper.OnDispose<IncomingFileTransfer>(disposing);
                    disposed = true;
                }
            }
        }
        
        private class OutcomingFileTransferInfo
        {
            public ulong Token;
            public string Receiver;
            public string Name;
            public long Size;
            public byte[] Hash;
            public int InitBlockSize;
        }

        private sealed class OutcomingFileTransfer : FileTransferBase, IOutcomingFileTransfer
        {
            private bool deleted = false;
            private bool disposed = false;
            private FileTransferId id;
            public override FileTransferDirection Direction { get { return FileTransferDirection.Out; } }
            public override FileTransferId Id { get { return id; } }
            public void SetId(FileTransferId newId) { id = newId; }
            public string Receiver { get; private set; }
            public ulong Token { get; private set; }
            public BlockFileReader Reader;
            
            public OutcomingFileTransfer(FileTransferManager owner, OutcomingFileTransferInfo info) : base(owner)
            {
                id = FileTransferId.InvalidId;
                State = FileTransferState.Waiting;
                Name = info.Name;
                Hash = info.Hash;
                BytesTotal = info.Size;
                BlockSize = info.InitBlockSize;
                Receiver = info.Receiver;
                Token = info.Token;
            }

            public override void Cancel()
            {
                IMessage msg;
                switch (State)
                {
                case FileTransferState.Success:
                case FileTransferState.Failure:
                case FileTransferState.Cancelled:
                    return;
                case FileTransferState.Waiting:
                    msg = new ClFileTransferRespond
                    {
                        Result = FileTransferRequestResult.Rejected,
                        SessionId = Id
                    };
                    break;
                case FileTransferState.Working:
                default:
                    msg = new CsFileTransferInterruption
                    {
                        SessionId = Id,
                        Token = Token,
                        Int = FileTransferInterruption.Cancel
                    };
                    break;
                }
                State = FileTransferState.Cancelled;
                Delete();
                Owner.client.Send(msg);
                Owner.OnTransferStateChanged(this);
            }

            public override void Delete()
            {
                if (deleted)
                    return;
                deleted = true;
                if (Id != FileTransferId.InvalidId)
                    Owner.delAssignedFts.Enqueue(Id);
                else
                    Owner.delPendingFts.Enqueue(Token);
            }
            
            protected override void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        if (Reader != null)
                        {
                            Reader.Dispose();
                            Reader = null;
                        }
                    }
                    DisposeHelper.OnDispose<OutcomingFileTransfer>(disposing);
                    disposed = true;
                }
            }
        }

        private readonly Client client;
        private readonly int blockSize;
        private ulong lastToken;
        // transfers with assigned id
        private readonly Dictionary<FileTransferId, FileTransferBase> assignedFts;
        private readonly ConcurrentQueue<FileTransferId> delAssignedFts;
        // transfers without id (waiting & out)
        private readonly Dictionary<ulong, OutcomingFileTransfer> pendingFts;
        private readonly ConcurrentQueue<OutcomingFileTransfer> newPendingFts;
        private readonly ConcurrentQueue<ulong> delPendingFts;
        
        public event FileTransferStateHandler TransferStateChanged;
        public event FileTransferRequestHandler TransferRequestReceived;
        public event FileTransferDataSentHandler DataSent;
        public event FileTransferDataReceivedHandler DataReceived;

        public FileTransferManager(Client client, int blockSize)
        {
            this.client = client;
            this.blockSize = blockSize;
            assignedFts = new Dictionary<FileTransferId, FileTransferBase>();
            delAssignedFts = new ConcurrentQueue<FileTransferId>();
            pendingFts = new Dictionary<ulong, OutcomingFileTransfer>();
            newPendingFts = new ConcurrentQueue<OutcomingFileTransfer>();
            delPendingFts = new ConcurrentQueue<ulong>();
        }

        private void OnTransferStateChanged(IFileTransfer ft)
        {
            if (TransferStateChanged != null)
                TransferStateChanged(ft);
        }

        private void OnTransferRequestReceived(IIncomingFileTransfer ft)
        {
            if (TransferRequestReceived != null)
                TransferRequestReceived(ft);
        }

        private void OnDataSent(IOutcomingFileTransfer ft)
        {
            if (DataSent != null)
                DataSent(ft);
        }

        private void OnDataReceived(IIncomingFileTransfer ft)
        {
            if (DataReceived != null)
                DataReceived(ft);
        }
        
        private static byte[] ComputeHash(string fileName)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024))
                {
                    return md5.ComputeHash(fs);
                }
            }
        }

        private static bool CompareHashes(byte[] a, byte[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        public IOutcomingFileTransfer Add(string receiver, string fileName)
        {
            var token = lastToken++;
            var fi = new FileInfo(fileName);
            var hash = ComputeHash(fileName);
            var info = new OutcomingFileTransferInfo
            {
                Token = token,
                Receiver = receiver,
                Name = fileName,
                Size = fi.Length,
                Hash = hash,
                InitBlockSize = blockSize
            };
            var ft = new OutcomingFileTransfer(this, info);
            newPendingFts.Enqueue(ft);
            return ft;
        }
        
        public void OnMessage(IMessage msg)
        {
            // add new case to AppController.OnMessage when adding new case here
            switch (msg.Id)
            {
            case MessageId.SvFileTransferRequest:
                OnSvFileTransferRequest(msg as SvFileTransferRequest);
                break;
            case MessageId.SvFileTransferResult:
                OnSvFileTransferResult(msg as SvFileTransferResult);
                break;
            case MessageId.CsFileTransferData:
                OnCsFileTransferData(msg as CsFileTransferData);
                break;
            case MessageId.CsFileTransferVerificationResult:
                OnCsFileTransferVerificationResult(msg as CsFileTransferVerificationResult);
                break;
            case MessageId.CsFileTransferInterruption:
                OnCsFileTransferInterruption(msg as CsFileTransferInterruption);
                break;
            }
        }

        public void Update()
        {
            foreach (var ftpair in assignedFts)
            {
                IFileTransfer ft = ftpair.Value;
                if (ft.State != FileTransferState.Working)
                    continue;
                switch (ft.Direction)
                {
                case FileTransferDirection.In:
                    ProcessIncomingTransfer(ft as IncomingFileTransfer);
                    break;
                case FileTransferDirection.Out:
                    ProcessOutcomingTransfer(ft as OutcomingFileTransfer);
                    break;
                }
            }
            ProcessNewSessions();
            ProcessDeletedSessions();
        }

        private void ProcessIncomingTransfer(IncomingFileTransfer ift)
        {
            if (ift.Writer != null) // already processed
                return;
            ift.Writer = new BlockFileWriter(ift.Name, ift.BlockSize, ift.BytesTotal);
            var respond = new ClFileTransferRespond
            {
                Result = FileTransferRequestResult.Accepted,
                SessionId = ift.Id,
                BlockSize = ift.BlockSize
            };
            client.Send(respond);
        }

        private void ProcessOutcomingTransfer(OutcomingFileTransfer oft)
        {
            if (oft.Reader.Eof)
            {
                // waiting for verification result - skip
                return;
            }
            var lastBlock = oft.Reader.CurrentBlock == oft.Reader.BlockCount - 1;
            var tmpBlockSize = oft.BlockSize;
            if (lastBlock)
                tmpBlockSize -= oft.Reader.Padding;
            var msg = new CsFileTransferData
            {
                SessionId = oft.Id,
                Data = new byte[tmpBlockSize]
            };
            oft.Reader.Read(msg.Data);
            oft.BytesDone += tmpBlockSize;
            oft.BlocksDone = oft.Reader.CurrentBlock;
            client.Send(msg);
            OnDataSent(oft);
        }

        private void ProcessNewSessions()
        {
            OutcomingFileTransfer ft;
            while (newPendingFts.TryDequeue(out ft))
            {
                pendingFts.Add(ft.Token, ft);
                var msg = new ClFileTransferRequest
                {
                    Username = ft.Receiver,
                    FileName = ft.Name,
                    FileSize = ft.BytesTotal,
                    FileHash = ft.Hash,
                    BlockSize = ft.BlockSize,
                    Token = ft.Token
                };
                client.Send(msg);
            }
        }

        private void ProcessDeletedSessions()
        {
            ulong token;
            while (delPendingFts.TryDequeue(out token))
            {
                OutcomingFileTransfer ft;
                if (pendingFts.TryGetValue(token, out ft))
                    ft.Dispose();
                pendingFts.Remove(token);
            }
            FileTransferId sid;
            while (delAssignedFts.TryDequeue(out sid))
            {
                FileTransferBase ft;
                if (assignedFts.TryGetValue(sid, out ft))
                    ft.Dispose();
                assignedFts.Remove(sid);
            }
        }

        private void OnSvFileTransferRequest(SvFileTransferRequest msg)
        {
            var info = new IncomingFileTransferInfo
            {
                Id = msg.SessionId,
                Sender = msg.Username,
                Name = msg.FileName,
                Size = msg.FileSize,
                Hash = msg.FileHash,
                InitBlockSize = Math.Min(blockSize, msg.BlockSize)
            };
            var ft = new IncomingFileTransfer(this, info);
            // XXX: check key presence
            assignedFts.Add(ft.Id, ft);
            OnTransferRequestReceived(ft);
        }

        private void OnSvFileTransferResult(SvFileTransferResult msg)
        {
            OutcomingFileTransfer oft;
            if (!pendingFts.TryGetValue(msg.Token, out oft))
            {
                // XXX: log 'invalid token'
                return;
            }
            switch (msg.Result)
            {
            case FileTransferRequestResult.Accepted:
                pendingFts.Remove(msg.Token);
                oft.SetId(msg.SessionId);
                oft.State = FileTransferState.Working;
                oft.BlockSize = msg.BlockSize;
                oft.Reader = new BlockFileReader(oft.Name, oft.BlockSize);
                assignedFts.Add(oft.Id, oft);
                break;
            default: // rejected or ...
                oft.State = FileTransferState.Cancelled;
                oft.Delete();
                break;
            }
            OnTransferStateChanged(oft);
        }

        private void OnCsFileTransferData(CsFileTransferData msg)
        {
            FileTransferBase ft;
            if (!assignedFts.TryGetValue(msg.SessionId, out ft))
            {
                // XXX: log 'invalid session id'
                return;
            }
            var ift = ft as IncomingFileTransfer;
            if (ift == null)
            {
                // XXX: log
                return;
            }
            if (ift.Writer == null)
            {
                // XXX: log
                return;
            }
            ift.Writer.Write(msg.Data);
            ift.BytesDone += msg.Data.Length;
            ift.BlocksDone = ift.Writer.CurrentBlock;
            // XXX: send confirmation respond for each N blocks
            OnDataReceived(ift);
            // all data received: verify checksum, send verification result and delete session
            if (ift.Writer.Eof)
            {
                ift.Writer.Close();
                var hash = ComputeHash(ift.Name);
                var success = CompareHashes(hash, ift.Hash);
                var ftvr = success ? FileTransferVerificationResult.Success :
                    FileTransferVerificationResult.ChecksumMismatch;
                var result = new CsFileTransferVerificationResult
                {
                    Result = ftvr,
                    SessionId = ift.Id
                };
                client.Send(result);
                ApplyVerificationResult(ift, ftvr);
            }
        }

        private void OnCsFileTransferVerificationResult(CsFileTransferVerificationResult msg)
        {
            FileTransferBase ft;
            if (!assignedFts.TryGetValue(msg.SessionId, out ft))
            {
                // XXX: log 'invalid session id'
                return;
            }
            ApplyVerificationResult(ft, msg.Result);
        }

        private void OnCsFileTransferInterruption(CsFileTransferInterruption msg)
        {
            FileTransferBase ft;
            if (!assignedFts.TryGetValue(msg.SessionId, out ft))
            {
                // XXX: log 'invalid sid'
                return;
            }
            switch (msg.Int)
            {
            case FileTransferInterruption.Cancel:
            default:
                ft.State = FileTransferState.Cancelled;
                ft.Delete();
                break;
            }
            OnTransferStateChanged(ft);
        }

        private void ApplyVerificationResult(FileTransferBase ft, FileTransferVerificationResult result)
        {
            switch (result)
            {
            case FileTransferVerificationResult.Success:
                ft.State = FileTransferState.Success;
                break;
            case FileTransferVerificationResult.ChecksumMismatch:
                ft.State = FileTransferState.Failure;
                ft.ErrorMessage = "Checksum mismatch";
                break;
            default:
                ft.State = FileTransferState.Failure;
                ft.ErrorMessage = "Unknown failure";
                break;
            }
            ft.Delete();
            OnTransferStateChanged(ft);
        }
    }
}
