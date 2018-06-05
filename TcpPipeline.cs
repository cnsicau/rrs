using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
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
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

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
            if (Interlocked.Increment(ref interrupting) > 1)
                return; // 已在中止处理忽略

            if (stream == null)
            {
                using (socket)
                { interrupted?.Invoke(this, EventArgs.Empty); }
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

        void IPipeline.Interrupte() { Interrupte(); }

        #region Input

        void IPipeline.Input<TState>(IOCallback<TState> callback, TState state)
        {
            if (!input.Disposed)
                throw new InvalidOperationException("Current packet is not disposed.");

            try
            {
                GetStream().BeginRead(packetBuffer, 0, packetBuffer.Length, CompleteInput<TState>, new object[] { callback, state });
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }

        void CompleteInput<TState>(IAsyncResult asr)
        {
            try
            {
                var size = GetStream().EndRead(asr);
                Console.WriteLine($"receive {size} bytes from {this}.");
                if (size == 0)
                    Interrupte();
                else
                {
                    input.SetBufferSize(size);
                    var args = (object[])asr.AsyncState;
                    ((IOCallback<TState>)args[0])(this, input, (TState)args[1]);
                }
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }
        #endregion

        #region Output

        void IPipeline.Output<TState>(IPacket packet, IOCallback<TState> callback, TState state)
        {
            if (packet.Disposed)
                throw new InvalidOperationException("Packet is disposed.");

            var args = new object[] { callback, packet, state, 0 };

            if (Interlocked.Exchange(ref outputting, 1) == 1)
            {
                lock (queueLocker)
                {
                    if (outputQueue == null)
                        outputQueue = new Queue<object[]>(1);
                    outputQueue.Enqueue(args);
                }
                return;
            }

            packet.Read(WritePacketData<TState>, args);
        }

        void WritePacketData<TState>(byte[] buffer, int size, object[] args)
        {
            var callback = (IOCallback<TState>)args[0];
            var packet = (IPacket)args[1];
            var state = (TState)args[2];

            if (size == BufferPacket.NoData) // 包中数据已读取完毕
            {
                callback(this, packet, state);
                object[] next = null;
                lock (queueLocker)
                {
                    if (outputQueue != null && outputQueue.Count > 0)
                    {
                        next = outputQueue.Dequeue();
                    }
                    else
                    {
                        Interlocked.Exchange(ref outputting, 0);
                    }
                }
                if (next != null)// 存在后续时继续发送
                    ((IPacket)next[1]).Read(WritePacketData<TState>, next);
                return;
            }

            args[3] = size; // 保存 Size

            try
            { GetStream().BeginWrite(buffer, 0, size, CompleteOutput<TState>, args); }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }

        void CompleteOutput<TState>(IAsyncResult asr)
        {
            try
            {
                GetStream().EndWrite(asr);

                var args = (object[])asr.AsyncState;
                Console.WriteLine($"send {args[3]} bytes to {this}.");
                ((IPacket)args[1]).Read(WritePacketData<TState>, args);// 继续包的后续内容发送
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
        }
        #endregion

        public override string ToString() { return pipelineName; }
    }
}
