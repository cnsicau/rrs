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

        public void Connect<TState>(Action<IPipeline, TState> callback, TState state = default(TState))
        {
            var socket = base.Socket;
            socket.BeginConnect(address, port, OnConnected<TState>, new object[] { callback, state });
        }

        void OnConnected<TState>(IAsyncResult asr)
        {
            try
            {
                Socket.EndConnect(asr);
                pipelineName = Socket.LocalEndPoint + "=>" + Socket.RemoteEndPoint;
                var args = (object[])asr.AsyncState;
                ((Action<IPipeline, TState>)args[0])(this, (TState)args[1]);
            }
            catch
            {
                Interrupte();
            }
        }
    }
}