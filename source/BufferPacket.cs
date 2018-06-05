using System;

namespace Rrs
{
    /// <summary>
    /// 
    /// </summary>
    public class BufferPacket : IPacket
    {
        public const int NoData = 0;
        private int size = NoData;
        private readonly byte[] buffer;
        private readonly IPipeline source;
        private bool disposed = true;

        /// <summary>
        /// 指定内容构建
        /// </summary>
        /// <param name="source"></param>
        /// <param name="buffer"></param>
        public BufferPacket(IPipeline source, byte[] buffer)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            this.source = source;
            this.buffer = buffer;
        }

        /// <summary>
        /// 指定大小构建
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bufferSize"></param>
        public BufferPacket(IPipeline source, int bufferSize) : this(source, new byte[bufferSize]) { }

        IPipeline IPacket.Source { get { return source; } }

        void IPacket.Read<TState>(ReadCallback<TState> callback, TState state)
        {
            var size = this.size;
            this.size = NoData;                     // 已读取后，清空缓冲区
            this.disposed = true;                   // 自动释放
            callback(new PacketData(this, buffer, size), state);
        }

        /// <summary>
        /// 设置缓冲区数据内容长度
        /// </summary>
        /// <param name="size"></param>
        public void SetBufferSize(int size)
        {
            if (size < 0 || size > buffer.Length)
                throw new IndexOutOfRangeException("size");
            disposed = false;
            this.size = size;
        }

        /// <summary>
        /// 获取缓冲区
        /// </summary>
        public byte[] Buffer { get { return buffer; } }

        /// <summary>
        /// 获取是否已销毁
        /// </summary>
        public bool Disposed { get { return disposed; } }

        void IDisposable.Dispose()
        {
            if (!disposed)
            {
                size = NoData;
                disposed = true;
            }
        }
    }
}
