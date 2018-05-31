using System;
using System.Collections.Generic;
using System.Text;

namespace rrs
{
    /// <summary>
    /// 信息头说明
    ///  位       码     说明
    /// 0 - 3     MAGIC  标识头  4位 (1111) 15
    /// 4 - 6     VER    版本    3位 (010)  2
    /// 7 - 9     TYPE   类型    3位  TunnelPacketType 枚举值
    /// 10 - 23   SIZE   长度    14位 1 - 8192
    /// </summary>
    public class TunnelPacket : IPacket
    {
        public const int HeaderSize = 3;

        public const int MaxDataSize = 8192;

        private readonly TunnelPipeline pipeline;

        public const int MagicValue = 15;       // 1111
        public const int VersionValue = 2;      // 010

        //   0 - 4095      4096 - 4102
        //  DATA BLOCK     HEADER BLOCK
        private byte[] buffer = new byte[MaxDataSize + HeaderSize];
        private int size;
        private bool disposed;

        public TunnelPacket(TunnelPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        public int Magic { get; set; }

        public int Version { get; set; }

        public TunnelPacketType Type { get; set; }

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
