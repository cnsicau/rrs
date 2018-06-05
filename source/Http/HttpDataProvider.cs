using System;
using System.Diagnostics;

namespace Rrs.Http
{
    public class HttpDataProvider : PacketDataProvider
    {
        public const int MaxLineSize = 8192;
        protected readonly IPipeline pipeline;
        private readonly char[] buffer = new char[MaxLineSize];
        private int bufferOffset;

        private PacketData data;
        private int dataOffset;

        public HttpDataProvider(HttpPipeline pipeline) : base(pipeline.TransPipeline)
        {
            this.pipeline = pipeline;
        }

        /// <summary>
        /// 读取一行
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public void ReadLine<TState>(LineCallback<TState> callback, TState state)
        {
            if (data != null && dataOffset < data.Size) // 存在可用
            {
                var packet = data.Buffer;
                for (; dataOffset < data.Size; dataOffset++)
                {
                    if (bufferOffset == MaxLineSize) // 读取至超出最大行数据时，中止管道处理
                    {
                        Trace.TraceError("Line buffer is overflow.");
                        pipeline.Interrupte();
                        return;
                    }
                    if (packet[dataOffset] == '\n' && bufferOffset > 0 && buffer[bufferOffset - 1] == '\r') // 解析至 \r\n
                    {
                        dataOffset++;                 // packet缓冲区指针向后移动指向 \n 的后续字符
                        var length = bufferOffset - 1;  /*char \r*/
                        bufferOffset = 0;               // 重置缓冲
                        callback(pipeline, buffer, length, state);
                        return;
                    }
                    else
                    {
                        buffer[bufferOffset++] = (char)packet[dataOffset];
                    }
                }
            }
            GetPacketData(MaxLineSize, ProcessData<TState>, new object[] { callback, state });
        }

        private void ProcessData<TState>(PacketData data, object[] args)
        {
            this.data = data;
            dataOffset = 0;
            ReadLine((LineCallback<TState>)args[0], (TState)args[1]);
        }

        public void ReadContent<TState>(int size, ReadCallback<TState> callback, TState state)
        {
            if (data != null && dataOffset < data.Size) // 存在可用
            {
                data.Discard(dataOffset);
                dataOffset = size;
                callback(size < data.Size ? data.CreateSubData(size) : data, state);
            }
            else
            {
                GetPacketData(size, ProcessContent<TState>, new object[] { size, callback, state });
            }
        }

        private void ProcessContent<TState>(PacketData data, object[] args)
        {
            this.data = data;
            dataOffset = 0;

            ReadContent((int)args[0], (ReadCallback<TState>)args[1], (TState)args[2]);
        }
    }
}
