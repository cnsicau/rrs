using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Rrs.Http
{
    [DebuggerDisplay("{Method,nq} {Uri,nq} {ProtocolVersion,nq}")]
    public class HttpRequest : IPacket
    {
        private IList<HttpHeader> headers;
        private readonly HttpPipeline source;
        private IHttpBody body;
        private HttpDataProvider dataProvider;

        private bool disposed;

        public HttpRequest(HttpPipeline source)
        {
            this.source = source;
        }

        /// <summary>
        /// HTTP 方法
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 请求URI
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// HTTP 版本
        /// </summary>
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// 获取 HTTP 头
        /// </summary>
        public IList<HttpHeader> Headers
        {
            get { return headers ?? (headers = new List<HttpHeader>()); }
        }

        /// <summary>
        /// 内容长度
        /// </summary>
        public int ContentLength { get; set; }

        /// <summary>
        /// 是否多内容传输
        /// </summary>
        public bool Mulitpart { get; set; }

        /// <summary>
        /// 保持连接
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        public void SetBody(IHttpBody body) { this.body = body; }

        #region Packet Members

        void IPacket.Read<TState>(ReadCallback<TState> callback, TState state)
        {
            if (body == null)
            {
                callback(new PacketData(this, null, 0), state);
            }
            else
            {
                body.Read(callback, state);
            }
        }

        IPipeline IPacket.Source { get { return source; } }

        bool IPacket.Disposed { get { return disposed; } }

        void IDisposable.Dispose()
        {
            if (disposed)
            {
                disposed = true;
            }
        }

        #endregion

        #region Internal & Private Methods

        internal void Construct<TState>(IOCallback<TState> callback, TState state)
        {
            if (dataProvider == null) dataProvider = new HttpDataProvider(source);

            dataProvider.ReadLine(ParseProtocol<TState>, new object[] { callback, state });

        }

        void ParseProtocol<TState>(IPipeline pipeline, char[] buffer, int length, object[] args)
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
                        Method = new string(buffer, 0, i);
                        uri = i + 1;
                    }
                    else
                    {
                        Uri = new string(buffer, uri, i - uri);
                        ProtocolVersion = new string(buffer, i + 1, length - i - 1);
                        break;
                    }
                }
            }
            if (uri == -1)
                throw new InvalidOperationException("invalid protocol.");

            dataProvider.ReadLine(ParseHeader<TState>, args);
        }

        void ParseHeader<TState>(IPipeline pipeline, char[] buffer, int length, object[] args)
        {
            if (length == 0)  // 仅解析到 '\r'，说明当前位置为Header 与 Body 间的内容分隔符“空行”
            {
                foreach (var header in headers)
                {
                    if (header.Name.Equals("content-length", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ContentLength = Convert.ToInt32(header.Value);
                    }
                    else if (header.Name.Equals("content-type", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Mulitpart = header.Value.IndexOf("multipart/", StringComparison.CurrentCultureIgnoreCase) >= 0;
                    }
                }

                if (ContentLength > 0)
                    body = new DataProviderBody(ContentLength, dataProvider);

                ((IOCallback<TState>)args[0])(pipeline, this, (TState)args[1]); // 信息头解析完成return回调
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
                    Headers.Add(new HttpHeader(name, value));
                    if (Headers.Count > 100) // 太多 Header 头
                    {
                        pipeline.Interrupte();
                        return;
                    }
                    // 继续读取更多 Header
                    dataProvider.ReadLine(ParseHeader<TState>, args);
                    return;
                }
            }

            throw new InvalidOperationException("Invalid Header :" + new string(buffer, 0, length));
        }
        #endregion
    }
}
