using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

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

        public SocketPipeline(Socket socket, bool ownedSocket = true)
        {
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
            using (stream)
            {
                if (interrupted != null)
                {
                    interrupted(this, EventArgs.Empty);
                    interrupted = null;
                }
            }
        }

        void IDisposable.Dispose()
        {
            Interrupte();
        }

        void IPipeline.Input<TState>(Action<IPacket, TState> callback, TState state)
        {
            if (!this.input.Disposed) throw new InvalidOperationException("input packet is not disposed.");

            try { stream.BeginRead(((IPacket)input).Buffer, 0, SocketPacket.BufferSize, CompleteInput<TState>, new object[] { callback, state }); }
            catch { using (this) { throw; } }
        }

        void CompleteInput<TState>(IAsyncResult asr)
        {
            try { input.SetSize(stream.EndRead(asr)); }
            catch { using (this) { throw; } }
            ((Action<IPacket, TState>)((object[])asr.AsyncState)[0])(input, (TState)((object[])asr.AsyncState)[1]);
        }

        void IPipeline.Interrupte()
        {
            throw new NotImplementedException();
        }

        void IPipeline.Output<TState>(IPacket packet, Action<TState> callback, TState state)
        {
            if (packet.Disposed) throw new InvalidOperationException("packet is disposed.");

            try { stream.BeginWrite(packet.Buffer, 0, packet.Size, CompleteOutput<TState>, new object[] { callback, state }); }
            catch { using (this) { throw; } }
        }

        void CompleteOutput<TState>(IAsyncResult asr)
        {
            try { stream.EndWrite(asr); }
            catch { using (this) { throw; } }
            ((Action<TState>)((object[])asr.AsyncState)[0])((TState)((object[])asr.AsyncState)[1]);
        }
    }
}
