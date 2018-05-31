using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;

namespace rrs
{
    /// <summary>
    /// 
    /// </summary>
    public class SocketPipeline : IPipeline
    {
        private readonly Lazy<Stream> stream;
        private readonly Packet input;
        private readonly Socket socket;
        private EventHandler interrupted;
        private int interrupting = 0;
        protected string pipelineName;

        public SocketPipeline(Socket socket)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));

            pipelineName = socket.RemoteEndPoint + "=>" + socket.LocalEndPoint;
            this.stream = new Lazy<Stream>(CreateStream);
            this.input = new Packet(this);
            this.socket = socket;
        }
        Stream CreateStream() { return OnCreateStream(); }

        protected virtual Stream OnCreateStream() { return new NetworkStream(socket, true); }

        event EventHandler IPipeline.Interrupted
        {
            add { interrupted += value; }
            remove { interrupted -= value; }
        }

        public Socket Socket { get { return socket; } }

        public void Interrupte()
        {
            if (Interlocked.Increment(ref interrupting) > 1) return; // 已在中止处理忽略

            if (!stream.IsValueCreated)
            {
                using (socket) { interrupted?.Invoke(this, EventArgs.Empty); }
            }
            else
            {
                using (stream.Value)
                {
                    stream.Value.Flush();
                    stream.Value.Close();
                    interrupted?.Invoke(this, EventArgs.Empty);
                }
            }
            interrupted = null;
        }

        void IDisposable.Dispose() { Interrupte(); }

        void IPipeline.Input<TState>(IOCompleteCallback<TState> callback, TState state)
        {
            if (!this.input.Disposed) throw new InvalidOperationException("input packet is not disposed.");

            try { stream.Value.BeginRead(((IPacket)input).Buffer, 0, Packet.BufferSize, CompleteInput<TState>, new object[] { callback, state }); }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
            catch { Interrupte(); throw; }
        }

        void CompleteInput<TState>(IAsyncResult asr)
        {
            try
            {
                var size = stream.Value.EndRead(asr);
                if (size == 0) Interrupte();
                else
                {
                    input.Relive(size);
                    var args = (object[])asr.AsyncState;
                    ((IOCompleteCallback<TState>)args[0])(this, input, (TState)args[1]);
                }
            }
            catch (ObjectDisposedException) { using (this) { } }
            catch { using (this) { throw; } }
        }

        void IPipeline.Interrupte() { Interrupte(); }

        void IPipeline.Output<TState>(IPacket packet, IOCompleteCallback<TState> callback, TState state)
        {
            if (packet.Disposed) throw new InvalidOperationException("packet is disposed.");

            try { stream.Value.BeginWrite(packet.Buffer, 0, packet.Size, CompleteOutput<TState>, new object[] { callback, packet, state }); }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
            catch { Interrupte(); throw; }
        }

        void CompleteOutput<TState>(IAsyncResult asr)
        {
            try
            {
                stream.Value.EndWrite(asr);
                var args = (object[])asr.AsyncState;
                ((IOCompleteCallback<TState>)args[0])(this, (IPacket)args[1], (TState)args[2]);
            }
            catch (ObjectDisposedException) { Interrupte(); }
            catch { Interrupte(); throw; }
        }

        public override string ToString() { return pipelineName; }
    }
}
