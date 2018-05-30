using System;
using System.Net;
using System.Net.Sockets;

namespace rrs
{
    public class ClientSocketPipeline : SocketPipeline
    {
        private readonly IPAddress address;
        private readonly int port;

        public ClientSocketPipeline(IPAddress address, int port)
          : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            this.address = address;
            this.port = port;
        }

        public void Connect<TState>(PipelineCallback<TState> callback, TState state = default(TState))
        {
            var socket = base.Socket;
            socket.BeginConnect(address, port, OnConnected<TState>, new object[] { callback, state });
        }

        void OnConnected<TState>(IAsyncResult asr)
        {
            var args = (object[])asr.AsyncState;
            try
            {
                Socket.EndConnect(asr);
                pipelineName = Socket.LocalEndPoint + "=>" + Socket.RemoteEndPoint;
                OnConnected((PipelineCallback<TState>)args[0], (TState)args[1]);
            }
            catch
            {
                ((PipelineCallback<TState>)args[0])(this, false, (TState)args[1]);
                Interrupte();
            }
        }

        protected virtual void OnConnected<TState>(PipelineCallback<TState> callback, TState state = default(TState))
        {
            callback(this, true, state);
        }
    }
}