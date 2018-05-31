using System;
using System.Collections.Generic;
using System.Text;

namespace rrs
{
    public class TunnelPacketReader
    {
        private readonly TunnelPacket tunnelPacket;
        private readonly byte[] headerBytes = new byte[TunnelPacket.HeaderSize];   // 信息头内容
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
                    headerBytes[headerSize] = source[sourceOffset++];
                }

                if (headerSize < TunnelPacket.HeaderSize)
                {
                    return false;   // Header 不足
                }
                ///  位       码     说明
                /// 0 - 3     MAGIC  标识头  4位 (1111) 15
                /// 4 - 6     VER    版本    3位 (010)  2
                /// 7 - 9     TYPE   类型    3位  TunnelPacketType 枚举值
                /// 10 - 23   SIZE   长度    14位 1 - 8192
                ((IPacket)tunnelPacket).Relive(((headerBytes[1] & 0x3f) << 8) | headerBytes[2]);
                tunnelPacket.Type = (TunnelPacketType)((headerBytes[1] >> 6) | (headerBytes[0] & 1) << 2);
                tunnelPacket.Version = (0x7 & (headerBytes[0] >> 1));
                tunnelPacket.Magic = headerBytes[0] >> 4;
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
