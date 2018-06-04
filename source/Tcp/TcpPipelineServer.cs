using System;
using System.Net;
using System.Net.Sockets;

namespace Rrs.Tcp
{
    /// <summary>
    /// Socket TCP 管道
    /// </summary>
    public class TcpPipelineServer : IPipelineServer
    {
        private readonly Socket listen;
        private readonly int backlog;
        private bool disposing;

        public event EventHandler Disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public TcpPipelineServer(IPAddress address, int port, int backlog)
        {
            this.backlog = backlog;

            this.listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.listen.Bind(new IPEndPoint(address, port));
            this.listen.Listen(backlog);
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="accept"></param>
        /// <param name="state"></param>
        public void Run<TState>(ConnectCallback<TState> accept, TState state = default(TState))
        {
            if (!disposing)
                listen.BeginAccept(CompleteAccept<TState>, new object[] { accept, state });
        }

        void CompleteAccept<TState>(IAsyncResult asr)
        {
            var args = (object[])asr.AsyncState;
            var callback = (ConnectCallback<TState>)args[0];
            var state = (TState)args[1];

            try
            {
                // 继续下一周期
                Run(callback, state);
                var socket = listen.EndAccept(asr);
                OnCreatePipeline(socket, callback, state);
            }
            catch (ObjectDisposedException) { }
        }

        protected virtual void OnCreatePipeline<TState>(Socket socket, ConnectCallback<TState> callback, TState state)
        {
            callback(new TcpPipeline(socket), true, state);
        }

        public void Dispose()
        {
            if (!disposing)
            {
                disposing = true;
                Disposed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
