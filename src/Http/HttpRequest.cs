using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Http
{
    public class HttpRequest : IPacket
    {
        private IList<HttpHeader> headers;
        private IPipeline source;

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

        IPipeline IPacket.Source { get { return source; } }

        int IPacket.Size { get { return this.ContentLength; } }

        byte[] IPacket.Buffer => throw new NotImplementedException();

        bool IPacket.Disposed => throw new NotImplementedException();

        void IDisposable.Dispose() { }

        void IPacket.Relive(int size) { ContentLength = size; }

        #endregion
    }
}
