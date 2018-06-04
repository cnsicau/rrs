using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Rrs.Tcp
{
    [DebuggerDisplay("{socket.LocalEndPoint}=>{socket.RemoteEndPoint}")]
    public class TcpPipeline : IPipeline
    {
        private readonly Socket socket;
        private int interrupting;
        private readonly BufferPacket packet;
        private Stream stream;

        public TcpPipeline(Socket socket)
        {
            this.socket = socket;
            packet = new BufferPacket(this, socket.ReceiveBufferSize);
        }

        #region Protected Members
        /// <summary>
        /// 获取关联的套接字
        /// </summary>
        protected Socket Socket { get { return socket; } }
        /// <summary>
        /// 获取传输流
        /// </summary>
        /// <returns></returns>
        protected virtual Stream Stream
        {
            get
            {
                return stream ?? (stream = new NetworkStream(socket, true));
            }
        }

        /// <summary>
        /// 流中Input完成回调
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="asr"></param>
        protected virtual void OnInput<TState>(IAsyncResult asr)
        {
            try
            {
                var dataSize = Stream.EndRead(asr);
                if (dataSize > 0)
                {
                    packet.SetBufferSize(dataSize);
                    // 回调 Input => IOCallback
                    var args = (object[])asr.AsyncState;
                    ((IOCallback<TState>)args[0])(this, packet, (TState)args[1]);
                }
                else
                {
                    Interrupte();   // 收到 0 长度包
                }
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { }
        }

        /// <summary>
        /// Packet Read回调，用于准备数据并发送到流
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="size"></param>
        /// <param name="args"></param>
        private void OnRead<TState>(byte[] buffer, int size, object[] args)
        {
            if (size <= 0)
            {
                ((IOCallback<TState>)args[1])(this, (IPacket)args[0], (TState)args[2]);
            }
            else
            {
                try
                {
                    Stream.BeginWrite(buffer, 0, size, CompleteOutput<TState>, args);
                }
                catch (IOException) { Interrupte(); }
                catch (ObjectDisposedException) { }
            }
        }

        /// <summary>
        /// 完成数据块输出
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="asr"></param>
        private void CompleteOutput<TState>(IAsyncResult asr)
        {
            try
            {
                Stream.EndWrite(asr);
                var args = (object[])asr.AsyncState;
                ((IPacket)args[0]).Read(OnRead<TState>, args);  // 继续读取并发送包的后续内容
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { }
        }
        #endregion

        #region IPipeline Members
        /// <summary>
        /// 中断事件
        /// </summary>
        public event EventHandler Interrupted;

        public void Dispose() { Interrupte(); }

        public void Input<TState>(IOCallback<TState> callback, TState state = default(TState))
        {
            if (!packet.Disposed)
                throw new InvalidOperationException("Current packet is NOT disposed.");

            try
            {
                Stream.BeginRead(packet.Buffer, 0, packet.Buffer.Length, OnInput<TState>, new object[] { callback, state });
            }
            catch (IOException) { Interrupte(); }
            catch (ObjectDisposedException) { }
        }

        public void Interrupte()
        {
            // 已中断退出
            if (Interlocked.Exchange(ref interrupting, 1) == 1) return;
            Console.WriteLine("interrupted.");

            try
            {
                if (stream == null)
                    using (Socket) Socket.Shutdown(SocketShutdown.Both);
                else
                    using (stream) { stream.Flush(); }
            }
            catch (SocketException) { }
            catch (IOException) { }
            catch (ObjectDisposedException) { }

            Interrupted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="packet"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void Output<TState>(IPacket packet, IOCallback<TState> callback, TState state = default(TState))
        {
            packet.Read(OnRead<TState>, new object[] { packet, callback, state, 0 });
        }
        #endregion
    }
}