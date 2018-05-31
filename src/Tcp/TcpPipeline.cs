using System;
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
        private readonly Packet input;
        private readonly Socket socket;
        private EventHandler interrupted;
        private int interrupting = 0;
        protected string pipelineName;

        public TcpPipeline(Socket socket)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));

            pipelineName = socket.RemoteEndPoint + "=>" + socket.LocalEndPoint;
            this.input = new Packet(this);
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

            try { GetStream().BeginRead(((IPacket)input).Buffer, 0, Packet.BufferSize, CompleteInput<TState>, new object[] { callback, state }); }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
            catch { Interrupte(); throw; }
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
            catch { Interrupte(); throw; }
        }

        void IPipeline.Interrupte() { Interrupte(); }

        void IPipeline.Output<TState>(IPacket packet, IOCompleteCallback<TState> callback, TState state)
        {
            if (packet.Disposed) throw new InvalidOperationException("packet is disposed.");

            try { GetStream().BeginWrite(packet.Buffer, 0, packet.Size, CompleteOutput<TState>, new object[] { callback, packet, state }); }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { Interrupte(); }
            catch { Interrupte(); throw; }
        }

        void CompleteOutput<TState>(IAsyncResult asr)
        {
            try
            {
                GetStream().EndWrite(asr);
                var args = (object[])asr.AsyncState;
                ((IOCompleteCallback<TState>)args[0])(this, (IPacket)args[1], (TState)args[2]);
            }
            catch (ObjectDisposedException) { Interrupte(); }
            catch { Interrupte(); throw; }
        }

        public override string ToString() { return pipelineName; }
    }
}
