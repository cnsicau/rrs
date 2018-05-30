using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace rrs
{
    /// <summary>
    /// 
    /// </summary>
    public class SocketPipeline : IPipeline
    {
        private readonly NetworkStream stream;
        private readonly SocketPacket input;
        private EventHandler interrupted;
        private int interrupting = 0;
        private string pipelineName;

        public SocketPipeline(Socket socket, bool ownedSocket = true)
        {
            pipelineName = socket.RemoteEndPoint + "=>" + socket.LocalEndPoint;
            this.stream = new NetworkStream(socket, ownedSocket);
            this.input = new SocketPacket(this);
        }

        event EventHandler IPipeline.Interrupted
        {
            add { interrupted += value; }
            remove { interrupted -= value; }
        }

        public void Interrupte()
        {
            if (Interlocked.Increment(ref interrupting) > 1) return; // 已在中止处理忽略
            using (stream)
            {
                stream.Flush();
                stream.Close();

                if (interrupted != null)
                {
                    interrupted(this, EventArgs.Empty);
                    interrupted = null;
                }
            }
        }

        void IDisposable.Dispose() { Interrupte(); }

        void IPipeline.Input<TState>(IOCompleteCallback<TState> callback, TState state)
        {
            if (!this.input.Disposed) throw new InvalidOperationException("input packet is not disposed.");

            try { stream.BeginRead(((IPacket)input).Buffer, 0, SocketPacket.BufferSize, CompleteInput<TState>, new object[] { callback, state }); }
            catch (IOException) { using (this) { } }
            catch (ObjectDisposedException) { using (this) { } }
            catch { using (this) { throw; } }
        }

        void CompleteInput<TState>(IAsyncResult asr)
        {
            try
            {
                var size = stream.EndRead(asr);
                if (size == 0) using (this) { }
                else
                {
                    input.SetSize(size);
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

            try { stream.BeginWrite(packet.Buffer, 0, packet.Size, CompleteOutput<TState>, new object[] { callback, packet, state }); }
            catch (IOException) { using (this) { } }
            catch (ObjectDisposedException) { using (this) { } }
            catch { using (this) { throw; } }
        }

        void CompleteOutput<TState>(IAsyncResult asr)
        {
            try
            {
                stream.EndWrite(asr);
                var args = (object[])asr.AsyncState;
                ((IOCompleteCallback<TState>)args[0])(this, (IPacket)args[1], (TState)args[2]);
            }
            catch (ObjectDisposedException) { using (this) { } }
            catch { using (this) { throw; } }
        }

        public override string ToString() { return pipelineName; }
    }
}
