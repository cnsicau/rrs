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
            var args = new object[] { packet, callback, state };

            if (packet is TunnelPacket)
            {
                ((TunnelPacket)packet).ReadHeader(OutputHeader<TState>, args);
            }
            else
            {
                packet.Read(OnNormalRead<TState>, args);
            }
        }

        void OnNormalRead<TState>(PacketData data, object[] args)
        {
            if (data.Completed)
            {
                data.Packet.Dispose();
                ((IOCallback<TState>)args[1])(this, data.Packet, (TState)args[2]);
            }
            else
            {
                var tunnelPacket = new OutputTunnelPacket(this, header, TunnelPacketType.Data, data);
                Output(tunnelPacket, CompleteNormarlOutput<TState>, args);
            }
        }

        void CompleteNormarlOutput<TState>(IPipeline source, IPacket packet, object[] args)
        {
            ((IPacket)args[0]).Read(OnNormalRead<TState>, args);
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
