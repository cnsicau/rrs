using System;
using System.Net;
using System.Net.Sockets;

namespace Rrs.Tcp
{
    /// <summary>
    /// 
    /// </summary>
    public class TcpClientPipeline : TcpPipeline
    {
        private readonly IPAddress address;
        private readonly int port;

        /// <summary>
        /// 构建指定地址连接
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public TcpClientPipeline(IPAddress address, int port)
          : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            this.address = address;
            this.port = port;
        }


        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void Connect<TState>(ConnectCallback<TState> callback, TState state = default(TState))
        {
            var socket = base.Socket;
            socket.BeginConnect(address, port, CompleteConnect<TState>, new object[] { callback, state });
        }

        void CompleteConnect<TState>(IAsyncResult asr)
        {
            var args = (object[])asr.AsyncState;
            try
            {
                Socket.EndConnect(asr);
                OnConnected((ConnectCallback<TState>)args[0], (TState)args[1]);
            }
            catch
            {
                ((ConnectCallback<TState>)args[0])(this, false, (TState)args[1]);
                Interrupte();
            }
        }

        /// <summary>
        /// 连接成功后的额外处理，如 SSL 握手
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        protected virtual void OnConnected<TState>(ConnectCallback<TState> callback, TState state = default(TState))
        {
            callback(this, true, state);
        }
    }
}