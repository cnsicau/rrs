using System;

namespace Rrs
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
        /// 读取内容
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns>返回是否完成</returns>
        bool Read<TState>(PacketCallback<TState> callback, TState state = default(TState));

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
