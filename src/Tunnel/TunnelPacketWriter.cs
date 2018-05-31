using System;

namespace Rrs.Tunnel
{
    public class TunnelPacketWriter
    {
        private readonly TunnelPipeline tunnelPipeline;
        private readonly IPacket transPacket;
        /// <summary>待写出的包</summary>
        private IPacket outputPacket;
        private object callback;
        private object state;

        public TunnelPacketWriter(TunnelPipeline tunnelPipeline)
        {
            if (Packet.BufferSize < TunnelPacket.HeaderSize)
                throw new InvalidOperationException($"Packet.BufferSize < {TunnelPacket.HeaderSize}");
            this.tunnelPipeline = tunnelPipeline;
            transPacket = new Packet(tunnelPipeline);
        }

        public void Write<TState>(IPacket packet, IOCompleteCallback<TState> callback, TState state)
        {
            if (!transPacket.Disposed) throw new InvalidOperationException("trans is not completed");

            this.outputPacket = packet;
            this.callback = callback;
            this.state = state;

            var packetType = !(packet is TunnelPacket) ? (int)TunnelPacketType.Data : (int)((TunnelPacket)packet).Type;
            PacketHeaderSerializer.Serialize(packetType, packet.Size, transPacket.Buffer);

            var dataSize = Math.Min(Packet.BufferSize - TunnelPacket.HeaderSize, outputPacket.Size);
            transPacket.Relive(dataSize + TunnelPacket.HeaderSize);
            if (dataSize > 0) // 具备内容
            {
                Array.Copy(outputPacket.Buffer, 0, transPacket.Buffer, TunnelPacket.HeaderSize, dataSize);
            }
            tunnelPipeline.TransPipeline.Output(transPacket, CompleteTransOutput<TState>, dataSize);
        }

        void CompleteTransOutput<TState>(IPipeline pipeline, IPacket packet, int completeSize)
        {
            if (completeSize == outputPacket.Size)
            {
                transPacket.Dispose();
                ((IOCompleteCallback<TState>)callback)(tunnelPipeline, outputPacket, (TState)state);
            }
            else // 未完继续
            {
                var dataSize = Math.Min(Packet.BufferSize, outputPacket.Size - completeSize);
                Array.Copy(outputPacket.Buffer, completeSize, packet.Buffer, 0, dataSize);
                transPacket.Relive(dataSize);
                pipeline.Output(packet, CompleteTransOutput<TState>, dataSize + completeSize);
            }
        }
    }
}
