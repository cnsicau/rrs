using System;

namespace Rrs
{
    /// <summary>
    /// 服务端
    /// </summary>
    public interface IPipelineServer : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="accept"></param>
        /// <param name=""></param>
        void Run<TState>(PipelineCallback<TState> accept, TState state = default(TState));

        /// <summary>
        /// 
        /// </summary>
        event EventHandler Disposed;
    }
}
