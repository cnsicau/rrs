using System;
using System.Collections.Generic;
using System.Text;

namespace rrs
{
    public class TunnelPipeline : IPipeline
    {
        private readonly IPipeline pipeline;
        private readonly TunnelPacket inputPacket;
        private IPacket pendingPacket;
        private int bufferIndex = TunnelPacket.MaxDataSize;
        private int pendingIndex = 0;

        public TunnelPipeline(IPipeline pipeline)
        {
            this.pipeline = pipeline;
            inputPacket = new TunnelPacket(this);
        }

        public event EventHandler Interrupted
        {
            add { pipeline.Interrupted += value; }
            remove { pipeline.Interrupted -= value; }
        }

        public void Dispose() { pipeline.Dispose(); }

        public void Input<TState>(IOCompleteCallback<TState> callback, TState state = default(TState))
        {
            pipeline.Input(PackagePacket, state);
        }

        void PackagePacket<TState>(IPipeline pipeline, IPacket packet, TState state)
        {
            if (bufferIndex == TunnelPacket.MaxDataSize + TunnelPacket.HeaderSize) // 指向内容块
            {
                bufferIndex = 0;
            }

            if (bufferIndex > TunnelPacket.MaxDataSize) // 复制内容块
            {

            }
            else // 复制数据
            {

            }
            pendingPacket = packet;
        }

        public void Interrupte()
        {
            pipeline.Interrupte();
        }

        public void Output<TState>(IPacket packet, IOCompleteCallback<TState> callback, TState state = default(TState))
        {
            pipeline.Output(packet, callback, state);
        }
    }
}
