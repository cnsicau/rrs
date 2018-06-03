using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Http
{
    public class HttpRequest : IPacket
    {
        private readonly IList<HttpHeader> headers;
        private readonly IPipeline source;

        private bool disposed;

        public HttpRequest(IPipeline source)
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

        #region Packet Members

        void IPacket.Read<TState>(PacketCallback<TState> callback, TState state)
        {
            throw new NotImplementedException();
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
    }
}
