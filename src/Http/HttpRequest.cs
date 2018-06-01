using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Http
{
    public class HttpRequest
    {
        private IList<HttpHeader> headers;
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
        /// 是否分部传输
        /// </summary>
        public bool Mulitpart { get; set; }

        /// <summary>
        /// 保持连接
        /// </summary>
        public bool KeepAlive { get; set; }
    }
}
