using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Sdm.Core.Util;

namespace Sdm.Server
{
    internal delegate void PipeConsoleCommandHandler(string command, PipeConsole console);

    internal sealed class PipeConsoleServer : IDisposable
    {
        private bool disposed = false;
        private readonly NamedPipeServerStream svPipe;
        private readonly StreamWriter svPipeWriter;
        private readonly object readerLock = 0;
        private readonly PipeConsoleCommandHandler handler;

        public const string PipeName = "sdm_server";
        
        public PipeConsoleServer(PipeConsoleCommandHandler handler)
        {
            this.handler = handler;
            svPipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            svPipeWriter = new StreamWriter(svPipe, Encoding.ASCII);
            svPipe.BeginWaitForConnection(OnConnection, null);
        }
        
        private void OnConnection(IAsyncResult ar)
        {
            try
            { svPipe.EndWaitForConnection(ar); }
            catch (ObjectDisposedException)
            { return; }
            if (!svPipe.IsConnected)
                return;
            var thread = new Thread(ClientServiceLoop);
            thread.IsBackground = true;
            thread.Start();
        }

        private void OnTempPipeConnection(IAsyncResult ar)
        {
            var locker = ar.AsyncState;
            lock (locker)
            { Monitor.Pulse(locker); }
        }

        private void ClientServiceLoop()
        {
            var tmpPipeName = String.Format("{0}_tmp_{1}", PipeName, (ulong)DateTime.Now.ToBinary());
            lock (readerLock)
            {
                svPipeWriter.WriteLine(tmpPipeName);
                svPipeWriter.Flush();
                svPipe.WaitForPipeDrain();
                svPipe.Disconnect();
                // continue waiting
                svPipe.BeginWaitForConnection(OnConnection, null);
            }
            using (var pipe = new NamedPipeServerStream(tmpPipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Message, PipeOptions.Asynchronous))
            {
                object locker = 1;
                lock (locker)
                {
                    var ar = pipe.BeginWaitForConnection(OnTempPipeConnection, locker);
                    var timedOut = !Monitor.Wait(locker, 1000);
                    pipe.EndWaitForConnection(ar);
                    if (timedOut || !pipe.IsConnected)
                        return;
                }
                var clReader = new StreamReader(pipe, true);
                var clWriter = new StreamWriter(pipe, Encoding.UTF8);
                var proxy = new PipeConsole(clReader, clWriter);
                // fetch command from pipe while connected
                var cmd = clReader.ReadLine();
                if (cmd != null)
                    handler(cmd, proxy);
                pipe.WaitForPipeDrain();
                pipe.Disconnect();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    svPipe.Dispose();
                }
                DisposeHelper.OnDispose<PipeConsoleServer>(disposing);
                disposed = true;
            }
        }

        ~PipeConsoleServer() { Dispose(false); }

        #endregion
    }

    internal sealed class PipeConsole
    {
        private readonly StreamReader reader; // reserved for future use
        private readonly StreamWriter writer;

        public PipeConsole(StreamReader reader, StreamWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
        }
        
        public void WriteLine(string line)
        {
            writer.WriteLine(line);
            writer.Flush();
        }

        public void WriteLine(string format, params object[] args)
        {
            writer.WriteLine(format, args);
            writer.Flush();
        }
    }
}
