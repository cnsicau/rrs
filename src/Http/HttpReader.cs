using System;
using System.Diagnostics;

namespace Rrs.Http
{
    public abstract class HttpReader
    {
        /// <summary>
        /// 行读取回调
        /// </summary>
        /// <typeparam name="TLineState"></typeparam>
        /// <param name="buffer">缓冲区</param>
        /// <param name="length">buffer长度（除\r\n）</param>
        /// <param name="state">回调状态</param>
        protected delegate void ReadLineCallback<TLineState>(char[] buffer, int length, TLineState state);

        const int MaxLineSize = 8192;
        protected readonly IPipeline pipeline;
        private readonly char[] buffer = new char[MaxLineSize];

        private int bufferOffset;
        private IPacket packet;
        private int packetOffset;

        public HttpReader(IPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        /// <summary>
        /// 读取一行
        /// </summary>
        /// <typeparam name="TReadState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        protected void ReadLine<TReadState>(ReadLineCallback<TReadState> callback, TReadState state)
        {
            if (packet != null && packetOffset < packet.Size)
            {
                var packet = this.packet.Buffer;
                for (; packetOffset < this.packet.Size; packetOffset++)
                {
                    if (bufferOffset == MaxLineSize) // 读取至超出最大行数据时，中止管道处理
                    {
                        Trace.TraceError("line size is over " + MaxLineSize);
                        pipeline.Interrupte();
                        return;
                    }
                    if (packet[packetOffset] == '\n' && bufferOffset > 0 && buffer[bufferOffset - 1] == '\r') // 解析至 \r\n
                    {
                        packetOffset++;                 // packet缓冲区指针向后移动指向 \n 的后续字符
                        var length = bufferOffset - 1;  /*char \r*/
                        bufferOffset = 0;               // 重置缓冲
                        callback(buffer, length, state);

                        return;
                    }
                    else
                    {
                        buffer[bufferOffset++] = (char)packet[packetOffset];
                    }
                }
            }
            ReadPaket(OnReadLine<TReadState>, new object[] { callback, state });
        }

        void OnReadLine<TReadState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            ReadLine((ReadLineCallback<TReadState>)args[0], (TReadState)args[1]);
        }

        /// <summary>
        /// 继续读取报文
        /// </summary>
        /// <typeparam name="TReadState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        protected void ReadPaket<TReadState>(IOCompleteCallback<TReadState> callback, TReadState state)
        {
            if (packet != null)
            {
                // 存在有效报文继续处理
                if (packetOffset < packet.Size)
                {
                    callback(pipeline, this.packet, state);
                    return;
                }
                packet.Dispose();
            }

            pipeline.Input(OnPacketRead<TReadState>, new object[] { callback, state });
        }

        void OnPacketRead<TReadState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            packetOffset = 0;
            this.packet = packet;

            ((IOCompleteCallback<TReadState>)args[0])(pipeline, packet, (TReadState)args[1]);
        }
    }
}
