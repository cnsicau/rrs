using System;

namespace rrs
{
    public interface IPipeline : IDisposable
    {
        /// <summary>
        /// 管道中断
        /// </summary>
        event EventHandler Interrupted;

        /// <summary>
        /// 中断
        /// </summary>
        void Interrupte();

        /// <summary>
        /// 完成输入
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void Input<TState>(Action<IPacket, TState> callback, TState state);

        /// <summary>
        /// 开始输出
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="packet"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void Output<TState>(IPacket packet, Action<TState> callback, TState state);
    }
}
