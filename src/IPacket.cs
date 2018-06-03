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
        void Read<TState>(PacketCallback<TState> callback, TState state = default(TState));

        /// <summary>
        /// 包是否已使用完毕
        ///     using(packet) { ... }
        /// </summary>
        bool Disposed { get; }
    }
}
