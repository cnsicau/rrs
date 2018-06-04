using System;

namespace Rrs.Tunnel
{
    public class TunnelPipeline : IPipeline
    {
        private readonly IPipeline pipeline;
        private readonly InputTunnelPacket inputPacket;
        //private readonly TunnelPacketWriter writer;
        private bool interrupted = false;

        public TunnelPipeline(IPipeline pipeline)
        {
            this.pipeline = pipeline;
            inputPacket = new InputTunnelPacket(this);
            //writer = new TunnelPacketWriter(this);
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
            inputPacket.Build(callback, state);
        }

        public void Interrupte()
        {
            if (interrupted) return;

            //pipeline.Output()

            pipeline.Interrupte();
        }

        public void Output<TState>(IPacket packet, IOCallback<TState> callback, TState state = default(TState))
        {
            throw new NotImplementedException();
            //writer.Write(packet, callback, state);
        }
    }
}
