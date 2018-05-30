using System;
using System.Collections.Generic;
using System.Text;

namespace rrs
{
    /// <summary>
    /// 0         MAGIC  标识头  1 字节 R
    /// 1         VER    版本    1 字节 2
    /// 2         TYPE   类型    1 字节 TunnelPacketType 枚举值
    /// 3 - 5     LEN    长度    3 字节 1 - 4096（4096使用全0表示)
    /// </summary>
    public class TunnelPacket : IPacket
    {
        public const int HeaderSize = 6;

        public const int MaxDataSize = 4096;

        private readonly TunnelPipeline pipeline;
        //   0 - 4095      4096 - 4102
        //  DATA BLOCK     HEADER BLOCK
        private byte[] buffer = new byte[MaxDataSize + HeaderSize];
        private int size;
        private bool disposed;

        public TunnelPacket(TunnelPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        public char Magic { get; set; }

        public byte Version { get; set; }

        public TunnelPacketType Type { get; set; }

        public short Length { get; set; }

        IPipeline IPacket.Source { get { return pipeline; } }

        int IPacket.Size { get { return size; } }

        byte[] IPacket.Buffer { get { return buffer; } }

        bool IPacket.Disposed { get { return disposed; } }

        void IDisposable.Dispose() { disposed = true; }

        void IPacket.SetSize(int size)
        {
            if (size < 0 || size > MaxDataSize) throw new IndexOutOfRangeException("size");
            disposed = false;
            this.size = size;
        }
    }
}
