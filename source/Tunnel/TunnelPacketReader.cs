using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Tunnel
{
    public class TunnelPacketReader
    {
        private readonly TunnelPacket tunnelPacket;
        private readonly byte[] headerBuffer = new byte[TunnelPacket.HeaderSize];   // 信息头内容
        private int headerSize;

        private int packetBufferSize = 0;

        private int sourceOffset = 0;
        private int sourceSize;
        private byte[] source;

        public TunnelPacketReader(TunnelPacket tunnelPacket)
        {
            this.tunnelPacket = tunnelPacket;
        }

        /// <summary>
        /// 尝试读取
        /// </summary>
        /// <param name="tunnelPacket"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Read()
        {
            if (sourceOffset == sourceSize) return false;

            if (headerSize < TunnelPacket.HeaderSize)
            {
                for (; headerSize < TunnelPacket.HeaderSize && sourceOffset < sourceSize; headerSize++)
                {
                    headerBuffer[headerSize] = source[sourceOffset++];
                }

                if (headerSize < TunnelPacket.HeaderSize)
                {
                    return false;   // Header 不足
                }

                PacketHeaderSerializer.Deserialize(headerBuffer, tunnelPacket);

                if (tunnelPacket.Magic != TunnelPacket.MagicValue || tunnelPacket.Version != TunnelPacket.VersionValue)
                {
                    ((IPacket)tunnelPacket).Source.Interrupte();   // 中断无效报文连接
                }
            }

            var dataSize = Math.Min(sourceSize - sourceOffset, ((IPacket)tunnelPacket).Size - packetBufferSize);
            Array.Copy(source, sourceOffset, ((IPacket)tunnelPacket).Buffer, packetBufferSize, dataSize);
            packetBufferSize += dataSize;
            sourceOffset += dataSize;

            if (packetBufferSize == ((IPacket)tunnelPacket).Size)
            {
                // 重置
                headerSize = 0;
                packetBufferSize = 0;
                return true;
            }
            return false;
        }

        public void SetSource(byte[] source, int size)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (size <= 0 || size > source.Length) throw new ArgumentOutOfRangeException(nameof(size));

            this.sourceOffset = 0;
            this.source = source;
            this.sourceSize = size;
        }
    }
}
