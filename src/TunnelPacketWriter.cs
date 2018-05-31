using System;

namespace rrs
{
    public class TunnelPacketWriter
    {
        private static readonly byte firstByte = TunnelPacket.MagicValue * 0xf + TunnelPacket.VersionValue * 2;
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
            int packetType = (int)TunnelPacketType.Data;

            if (packet is TunnelPacket) packetType = (int)((TunnelPacket)packet).Type;

            var buffer = transPacket.Buffer;

            buffer[2] = (byte)(outputPacket.Size & 0xff);
            buffer[1] = (byte)(((outputPacket.Size & 0x3f00) >> 8) | (packetType << 6));
            buffer[0] = (byte)(firstByte | (packetType >> 8));

            var dataSize = Math.Min(Packet.BufferSize - 3, outputPacket.Size);
            transPacket.SetSize(dataSize + TunnelPacket.HeaderSize);
            if (dataSize > 0)
            {
                Array.Copy(outputPacket.Buffer, 0, buffer, TunnelPacket.HeaderSize, dataSize);
                tunnelPipeline.TransPipeline.Output(transPacket, CompleteTransOutput<TState>, dataSize);
            }
        }

        void CompleteTransOutput<TState>(IPipeline pipeline, IPacket packet, int completeSize)
        {
            if (completeSize == outputPacket.Size)
            {
                outputPacket.Dispose();
                transPacket.Dispose();
                ((IOCompleteCallback<TState>)callback)(tunnelPipeline, outputPacket, (TState)state);
            }
            else // 未完继续
            {
                var dataSize = Math.Min(Packet.BufferSize, outputPacket.Size - completeSize);
                Array.Copy(outputPacket.Buffer, completeSize, packet.Buffer, 0, dataSize);
                pipeline.Output(packet, CompleteTransOutput<TState>, dataSize + completeSize);
            }
        }
    }
}
