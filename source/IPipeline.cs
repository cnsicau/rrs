using System;

namespace Rrs
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
        /// 输入构建包
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void Input<TState>(IOCallback<TState> callback, TState state = default(TState));

        /// <summary>
        /// 输出包
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="packet"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        void Output<TState>(IPacket packet, IOCallback<TState> callback, TState state = default(TState));
    }
}
