using System;

namespace rrs
{
    /// <summary>
    /// 包
    /// </summary>
    public interface IPacket : IDisposable
    {
        /// <summary>
        /// 获取源管道
        /// </summary>
        IPipeline Source { get; }

        /// <summary>
        /// 包有效数据大小
        /// </summary>
        int Size { get; }

        /// <summary>
        /// 缓冲区
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// 包是否已释放
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// 激活数据包
        /// </summary>
        /// <param name="size">数据大小</param>
        void Relive(int size);
    }
}
