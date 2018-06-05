using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Tunnel
{
    class TunnelWriter<TState>
    {
        private readonly TunnelPipeline tunnelPipeline;
        private readonly IPacket packet;
        private readonly IOCallback<TState> callback;
        private readonly TState state;
        private readonly byte[] headerBuffer;

        public TunnelWriter(TunnelPipeline tunnelPipeline, byte[] headerBuffer, IPacket packet, IOCallback<TState> callback, TState state)
        {
            this.tunnelPipeline = tunnelPipeline;
            this.packet = packet;
            this.callback = callback;
            this.state = state;
            this.headerBuffer = headerBuffer;
        }

        void Complete(IPipeline a = null, IPacket b = null, object c = null)
        {
            callback(tunnelPipeline, packet, state);
        }
        /// <summary>
        /// 执行写操作
        /// </summary>
        public void Execute()
        {
            if (packet is TunnelPacket)
            {
                WriteTunnelPacket((TunnelPacket)packet, Complete, default(object));
            }
            else
            {
                ReadPacketForWriting();
            }
        }

        void ReadPacketForWriting(IPipeline a = null, IPacket b = null, object c = null)
        {
            packet.Read<object>(CompleteReadPacket, null);
        }

        void CompleteReadPacket(PacketData data, object arg)
        {
            if (data.Completed)
            {
                Complete();
                return;
            }
            var dataPacket = new OutputTunnelPacket(tunnelPipeline, headerBuffer, TunnelPacketType.Data, data);
            WriteTunnelPacket<object>(dataPacket, ReadPacketForWriting, null);
        }

        void WriteTunnelPacket<TWriteState>(TunnelPacket packet, IOCallback<TWriteState> callback, TWriteState state)
        {
            var pipeline = tunnelPipeline.TransPipeline;
            pipeline.Output(
                (BufferPacket)packet.HeaderData,
                CompleteHeaderWrite<TWriteState>,
                new object[] { packet, callback, state }
            );
        }

        void CompleteHeaderWrite<TWriteState>(IPipeline pipeline, IPacket headerPacket, object[] args)
        {
            pipeline.Output((IPacket)args[0], (IOCallback<TWriteState>)args[1], (TWriteState)args[2]);
        }
    }
}
