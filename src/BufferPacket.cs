using System;

namespace Rrs
{
    public class BufferPacket : IPacket
    {
        /// <summary>
        /// 默认包8K
        /// </summary>
        static public int BufferSize = 8192;

        private int size = 0;
        private readonly byte[] buffer = new byte[BufferSize];
        private readonly IPipeline source;
        private bool disposed = true;

        public BufferPacket(IPipeline source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            this.source = source;
        }

        IPipeline IPacket.Source { get { return source; } }

        bool IPacket.Read<TState>(PacketCallback<TState> callback, TState state)
        {
            callback(buffer, size, state);
            return false;
        }
        
        /// <summary>
        /// 设置缓冲区有效数据大小
        /// </summary>
        /// <param name="size"></param>
        public void Relive(int size)
        {
            if (size < 0 || size > BufferSize) throw new IndexOutOfRangeException("size");
            disposed = false;
            this.size = size;
        }

        /// <summary>
        /// 获取是否已销毁
        /// </summary>
        public bool Disposed { get { return disposed; } }

        void IDisposable.Dispose()
        {
            if (!disposed)
            {
                size = 0;
                disposed = true;
            }
        }
    }
}
