using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Rrs.Tcp
{
    /// <summary>
    /// 
    /// </summary>
    public class TcpPipeline : IPipeline
    {
        private Stream stream;
        private readonly BufferPacket input;
        private readonly Socket socket;
        private EventHandler interrupted;
        private int interrupting = 0;
        protected string pipelineName;
        private int outputting = 0;

        private Queue<object[]> outputQueue;
        private object queueLocker = new object();

        public TcpPipeline(Socket socket)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));

            pipelineName = socket.RemoteEndPoint + "=>" + socket.LocalEndPoint;
            this.input = new BufferPacket(this);
            this.socket = socket;
        }

        protected Stream GetStream()
        {
            if (stream == null)
                stream = CreateStream();

            return stream;
        }

        protected virtual Stream CreateStream() { return new NetworkStream(socket, true); }

        event EventHandler IPipeline.Interrupted
        {
            add { interrupted += value; }
            remove { interrupted -= value; }
        }

        public Socket Socket { get { return socket; } }

        public void Interrupte()
        {
            if (Interlocked.Increment(ref interrupting) > 1) return; // 已在中止处理忽略

            if (stream == null)
            {
                using (socket) { interrupted?.Invoke(this, EventArgs.Empty); }
            }
            else
            {
                using (stream)
                {
                    stream.Flush();
                    stream.Close();
                    interrupted?.Invoke(this, EventArgs.Empty);
                }
            }
            interrupted = null;
        }

        void IDisposable.Dispose() { Interrupte(); }

        void IPipeline.Input<TState>(IOCompleteCallback<TState> callback, TState state)
        {
            if (!input.Disposed) throw new InvalidOperationException("input packet is not disposed.");

            try { GetStream().BeginRead(((IPacket)input).Buffer, 0, BufferPacket.BufferSize, CompleteInput<TState>, new object[] { callback, state }); }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }

        void CompleteInput<TState>(IAsyncResult asr)
        {
            try
            {
                var size = GetStream().EndRead(asr);
                if (size == 0) Interrupte();
                else
                {
                    input.Relive(size);
                    var args = (object[])asr.AsyncState;
                    ((IOCompleteCallback<TState>)args[0])(this, input, (TState)args[1]);
                }
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }

        void IPipeline.Interrupte() { Interrupte(); }

        void IPipeline.Output<TState>(IPacket packet, IOCompleteCallback<TState> callback, TState state)
        {
            if (packet.Disposed) throw new InvalidOperationException("packet is disposed.");

            var args = new object[] { callback, packet, state };

            if (Interlocked.Exchange(ref outputting, 1) == 1)
            {
                lock (queueLocker)
                {
                    if (outputQueue == null) outputQueue = new Queue<object[]>(1);
                    outputQueue.Enqueue(args);
                }
                return;
            }
            ExecuteOutput<TState>(args);
        }

        void ExecuteOutput<TState>(object[] args)
        {
            var callback = (IOCompleteCallback<TState>)args[0];
            var packet = (IPacket)args[1];
            var state = (TState)args[2];

            try { GetStream().BeginWrite(packet.Buffer, 0, packet.Size, CompleteOutput<TState>, new object[] { callback, packet, state }); }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }

        void CompleteOutput<TState>(IAsyncResult asr)
        {
            try
            {
                GetStream().EndWrite(asr);

                var args = (object[])asr.AsyncState;
                ((IOCompleteCallback<TState>)args[0])(this, (IPacket)args[1], (TState)args[2]);

                object[] next = null;
                lock (queueLocker)
                {
                    if (outputQueue != null && outputQueue.Count > 0) next = outputQueue.Dequeue();
                }
                if (next == null) Interlocked.Exchange(ref outputting, 0);
                else ExecuteOutput<TState>(next);
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }

        public override string ToString() { return pipelineName; }
    }
}
