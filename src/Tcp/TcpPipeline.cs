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
        private readonly byte[] packetBuffer;
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
            packetBuffer = new byte[socket.ReceiveBufferSize];
            this.input = new BufferPacket(this, packetBuffer);
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

            try { GetStream().BeginRead(packetBuffer, 0, packetBuffer.Length, CompleteInput<TState>, new object[] { callback, state }); }
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
                    input.SetSize(size);
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

            var args = new object[] { callback, packet, state, 0 };

            if (Interlocked.Exchange(ref outputting, 1) == 1)
            {
                lock (queueLocker)
                {
                    if (outputQueue == null) outputQueue = new Queue<object[]>(1);
                    outputQueue.Enqueue(args);
                }
                return;
            }

            packet.Read(OutputPacket<TState>, args);
        }

        void OutputPacket<TState>(byte[] buffer, int size, object[] args)
        {
            var callback = (IOCompleteCallback<TState>)args[0];
            var packet = (IPacket)args[1];
            var state = (TState)args[2];

            args[3] = size; // 保存 Size

            try { GetStream().BeginWrite(buffer, 0, size, CompleteOutput<TState>, args); }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }

        void CompleteOutput<TState>(IAsyncResult asr)
        {
            try
            {
                GetStream().EndWrite(asr);

                var args = (object[])asr.AsyncState;
                if ((int)args[3] > 0)   // 包内容未完继续
                {
                    ((IPacket)args[1]).Read(OutputPacket<TState>, args);
                    return;
                }

                ((IOCompleteCallback<TState>)args[0])(this, (IPacket)args[1], (TState)args[2]);

                object[] next = null;
                lock (queueLocker)
                {
                    if (outputQueue != null && outputQueue.Count > 0) next = outputQueue.Dequeue();
                }
                if (next == null) Interlocked.Exchange(ref outputting, 0);

                ((IPacket)args[1]).Read(OutputPacket<TState>, next);
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }

        public override string ToString() { return pipelineName; }
    }
}
