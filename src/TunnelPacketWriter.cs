using System;

namespace rrs
{
    public class TunnelPacketWriter
    {
        private static readonly byte firstByte = (TunnelPacket.MagicValue << 4) + (TunnelPacket.VersionValue << 1);
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

            ///  位       码     说明
            /// 0 - 3     MAGIC  标识头  4位 (1111) 15
            /// 4 - 6     VER    版本    3位 (010)  2
            /// 7 - 9     TYPE   类型    3位  TunnelPacketType 枚举值
            /// 10 - 23   SIZE   长度    14位 1 - 8192
            buffer[2] = (byte)(outputPacket.Size & 0xff);  // 16 - 23
            buffer[1] = (byte)((outputPacket.Size >> 8) | ((0x7 & packetType) << 6));
            buffer[0] = (byte)(firstByte | (packetType >> 2));

            var dataSize = Math.Min(Packet.BufferSize - 3, outputPacket.Size);
            transPacket.Relive(dataSize + TunnelPacket.HeaderSize);
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
