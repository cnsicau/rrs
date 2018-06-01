using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Http
{
    public class HttpRequestReader
    {
        const int MaxHeadLineSize = 8192;
        private readonly IPipeline transPipeline;
        private IPacket currentPacket;
        private int bufferIndex;
        private HttpRequest request = new HttpRequest();

        private object completeHeader;    // IOCompleteCallback<TState>
        private object state;       // TState
        private char[] pending = new char[MaxHeadLineSize];
        private int pendingIndex = 0;


        public HttpRequestReader(IPipeline transPipeline)
        {
            this.transPipeline = transPipeline;
        }

        public HttpRequest Request { get { return request; } }

        public void Read<TState>(IOCompleteCallback<TState> completeHeader, TState state)
        {
            this.completeHeader = completeHeader;
            this.state = state;
            ReadPaket(ParseProtocol<TState>, (object)null);
        }

        void ParseProtocol<TState>(IPipeline pipeline, IPacket packet, object args)
        {
            if (bufferIndex < packet.Size) // 已读完，从传输中读取更多数据
            {
                var buffer = packet.Buffer;
                for (; pendingIndex < MaxHeadLineSize && bufferIndex < packet.Size; bufferIndex++)
                {
                    if (buffer[bufferIndex] == '\n' && pendingIndex > 0 && this.pending[pendingIndex - 1] == '\r') // \R\N
                    {
                        // 解析  GET  /uri HTTP/1.1
                        var uriIndex = -1;
                        for (int i = 0; i < pendingIndex - 1; i++)
                        {
                            var chr = pending[i];
                            if (char.IsWhiteSpace(chr))
                            {
                                if (uriIndex == -1)
                                {
                                    request.Method = new string(pending, 0, i);
                                    uriIndex = i + 1;
                                }
                                else
                                {
                                    request.Uri = new string(pending, uriIndex, i - uriIndex);
                                    request.ProtocolVersion = new string(pending, i + 1, pendingIndex - 1/*\r*/ - i - 1/*space*/);
                                    break;
                                }
                            }
                        }
                        bufferIndex++; //将指向跳过 \n
                        // 解析后续 HEADER
                        pendingIndex = 0;
                        ReadPaket(ParseHeader<TState>, new object[0]);
                        return;
                    }
                    else
                    {
                        pending[pendingIndex++] = (char)buffer[bufferIndex];
                    }
                }
            }

            ReadPaket(ParseProtocol<TState>, args);
        }

        void ParseHeader<TState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            if (bufferIndex < packet.Size) // 已读完，从传输中读取更多数据
            {
                var buffer = packet.Buffer;
                for (; pendingIndex < MaxHeadLineSize && bufferIndex < packet.Size; bufferIndex++)
                {
                    if (buffer[bufferIndex] == '\n' && pendingIndex > 0 && this.pending[pendingIndex - 1] == '\r') // \R\N
                    {
                        if (pendingIndex == 1)  // 仅解析到 '\r'，说明当前位置为Header 与 Body 间的内容分隔符“空行”
                        {
                            ((IOCompleteCallback<TState>)completeHeader)(transPipeline, packet, (TState)state); // 信息头解析完成return回调
                            bufferIndex++; //将指向跳过 \n
                            return;
                        }
                        // 解析  GET  /uri HTTP/1.1
                        for (int i = 0; i < pendingIndex - 1; i++)
                        {
                            var chr = pending[i];
                            if (chr == ':')
                            {
                                var header = new HttpHeader();
                                header.Name = new string(pending, 0, i);
                                header.Value = i == pendingIndex - 1 /*:是末尾结束符*/ ? null : new string(pending, i + 1, pendingIndex - 1/*\r*/ - i - 1/*:*/);
                                request.Headers.Add(header);
                                break;
                            }
                        }

                        bufferIndex++; //将指向跳过 \n
                        // 解析后续 HEADER
                        pendingIndex = 0;
                        ReadPaket(ParseHeader<TState>, new object[0]);
                        return;
                    }
                    else
                    {
                        pending[pendingIndex++] = (char)buffer[bufferIndex];
                    }
                }
            }

            ReadPaket(ParseHeader<TState>, args);
        }

        void ReadPaket<TState>(IOCompleteCallback<TState> callback, TState state)
        {
            if (currentPacket == null || bufferIndex == currentPacket.Size) // 耗尽或未开始
            {
                currentPacket?.Dispose();
                transPipeline.Input(CompleteReadPacket<TState>, new object[] { callback, state });
            }
            else
            {
                callback(transPipeline, currentPacket, state);
            }
        }

        void CompleteReadPacket<TState>(IPipeline pipeline, IPacket packet, object[] args)
        {
            bufferIndex = 0;
            currentPacket = packet;

            ((IOCompleteCallback<TState>)args[0])(pipeline, packet, (TState)args[1]);
        }
    }
}
