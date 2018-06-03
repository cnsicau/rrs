using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Http
{
    public class HttpRequestReader : HttpReader
    {
        private readonly HttpRequest request;

        private object completeHeader;    // IOCompleteCallback<TState>
        private object state;       // TState
        private readonly HttpPipeline httpPipeline;

        public HttpRequestReader(HttpPipeline httpPipeline) : base(httpPipeline.TransPipeline)
        {
            this.httpPipeline = httpPipeline;
            request = new HttpRequest(httpPipeline);
        }

        public HttpRequest Request { get { return request; } }

        public void Read<TState>(IOCompleteCallback<TState> completeHeader, TState state)
        {
            this.completeHeader = completeHeader;
            this.state = state;

            ReadLine(ParseProtocol<TState>, (object)null);
        }

        void ParseProtocol<TState>(char[] buffer, int length, object state)
        {
            // 解析  GET  /uri HTTP/1.1
            var uri = -1;
            for (int i = 0; i < length; i++)
            {
                var chr = buffer[i];
                if (char.IsWhiteSpace(chr))
                {
                    if (uri == -1)
                    {
                        request.Method = new string(buffer, 0, i);
                        uri = i + 1;
                    }
                    else
                    {
                        request.Uri = new string(buffer, uri, i - uri);
                        request.ProtocolVersion = new string(buffer, i + 1, length - i - 1);
                        break;
                    }
                }
            }
            if (uri == -1)
                throw new InvalidOperationException("invalid protocol.");

            ReadLine(ParseHeader<TState>, default(object));
        }

        void ParseHeader<TState>(char[] buffer, int length, object state)
        {
            if (length == 0)  // 仅解析到 '\r'，说明当前位置为Header 与 Body 间的内容分隔符“空行”
            {
                ((IOCompleteCallback<TState>)completeHeader)(pipeline, request, (TState)state); // 信息头解析完成return回调
                return;
            }
            // 解析  HeaderName: HeaderValue 格式的 Header 头
            for (int i = 0; i < length; i++)
            {
                var chr = buffer[i];
                if (chr == ':')
                {
                    var name = new string(buffer, 0, i);
                    var value = i == length - 1 /*:是末尾结束符*/ ? null : new string(buffer, i + 1, length - i - 1);
                    request.Headers.Add(new HttpHeader(name, value));

                    // 继续读取更多 Header
                    ReadLine(ParseHeader<TState>, default(object));
                    return;
                }
            }

            throw new InvalidOperationException("Invalid Header :" + new string(buffer, 0, length));
        }
    }
}
