using System;

namespace Rrs.Tunnel
{
    public class TunnelPipeline : IPipeline
    {
        private readonly IPipeline pipeline;
        private readonly InputTunnelPacket inputPacket;
        private bool interrupted = false;

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
            var args = new object[] { packet, callback, state };
            var tunnelPacket = packet as TunnelPacket;
            if (tunnelPacket == null)
                tunnelPacket = new OutputTunnelPacket(this, TunnelPacketType.Data, packet);

            tunnelPacket.ReadHeader(OutputHeader<TState>, args);
        }

        void OutputHeader<TState>(PacketData data, object[] args)
        {
            if (data.Size != TunnelPacket.HeaderSize) throw new InvalidOperationException("invalid header size.");

            var header = new BufferPacket(this, data.Buffer);
            header.SetBufferSize(data.Size);

            TransPipeline.Output(header, OnHeaderOutput<TState>, args);
        }

        void OnHeaderOutput<TState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            packet = (IPacket)args[0]; // Data
            TransPipeline.Output(packet, OnContentOutput<TState>, args);
        }

        void OnContentOutput<TState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            ((IOCallback<TState>)args[1])(this, packet, (TState)args[2]);
        }
    }
}
