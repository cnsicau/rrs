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
        /// <param name="state"></param>
        void Run<TState>(ConnectCallback<TState> accept, TState state = default(TState));

        /// <summary>
        /// 
        /// </summary>
        event EventHandler Disposed;
    }
}
