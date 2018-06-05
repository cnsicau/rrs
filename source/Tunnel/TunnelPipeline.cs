using System;

namespace Rrs.Tunnel
{
    public class TunnelPipeline : IPipeline
    {
        private readonly IPipeline pipeline;
        private readonly InputTunnelPacket inputPacket;
        private bool interrupted = false;
        private byte[] header = new byte[TunnelPacket.HeaderSize];

        public TunnelPipeline(IPipeline pipeline)
        {
            this.pipeline = pipeline;
            inputPacket = new InputTunnelPacket(this);
            pipeline.Interrupted += OnInterrupted;
        }

        public IPipeline TransPipeline { get { return pipeline; } }


        void OnInterrupted(object sender, EventArgs e)
        {
            interrupted = true;
            Interrupted?.Invoke(this, e);
        }

        public event EventHandler Interrupted;

        public void Dispose() { pipeline.Dispose(); }

        public void Input<TState>(IOCallback<TState> callback, TState state = default(TState))
        {
            inputPacket.Construct(callback, state);
        }

        public void Interrupte()
        {
            if (interrupted) return;

            pipeline.Interrupte();
        }

        public void Output<TState>(IPacket packet, IOCallback<TState> callback, TState state = default(TState))
        {
            if (packet.Disposed) throw new InvalidOperationException("packet is disposed.");

            new TunnelWriter<TState>(this, header, packet, callback, state).Execute();
        }
    }
}
