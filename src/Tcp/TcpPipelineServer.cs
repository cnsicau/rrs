﻿using System;
using System.Net;
using System.Net.Sockets;

namespace Rrs.Tcp
{
    class TcpPipelineServer : IPipelineServer
    {
        private readonly Socket listenSocket;
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

            this.listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.listenSocket.Bind(new IPEndPoint(address, port));
            this.listenSocket.Listen(backlog);
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="accept"></param>
        /// <param name="state"></param>
        public void Run<TState>(PipelineCallback<TState> accept, TState state = default(TState))
        {
            if (!disposing)
                listenSocket.BeginAccept(CompleteAccept<TState>, new object[] { accept, state });
        }

        void CompleteAccept<TState>(IAsyncResult asr)
        {
            var args = (object[])asr.AsyncState;
            var callback = (PipelineCallback<TState>)args[0];
            var state = (TState)args[1];

            try
            {

                // 继续下一周期
                Run(callback, state);

                var socket = listenSocket.EndAccept(asr);
                callback(new TcpPipeline(socket), true, state);
            }
            catch (ObjectDisposedException)
            {
                callback(null, false, state);
            }
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