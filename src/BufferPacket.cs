using System;

namespace Rrs
{
    public class BufferPacket : IPacket
    {
        private int size = 0;
        private readonly byte[] buffer;
        private readonly IPipeline source;
        private bool disposed = true;

        public BufferPacket(IPipeline source, byte[] buffer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            this.buffer = buffer;
            this.source = source;
        }

        IPipeline IPacket.Source { get { return source; } }

        void IPacket.Read<TState>(PacketCallback<TState> callback, TState state)
        {
            callback(buffer, size, state);
            size = 0;   // 已读取后，清空缓冲区
        }

        /// <summary>
        /// 重用包，默认已释放
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(int size)
        {
            if (size < 0 || size > buffer.Length) throw new IndexOutOfRangeException("size");
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
