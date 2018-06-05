using System.Collections.Generic;

namespace Rrs.Http
{
    /// <summary>
    /// HTTP 响应
    /// </summary>
    public class HttpResponse
    {
        private int contentLength;  // 响应内容长度(非 chunk 有效 )
        private int statusCode;     // 状态码
        private string status;      // 状态描述
        private string transferEncoding;         // 是否 Chunk 编码
        private IList<HttpHeader> headers;

        public int ContentLength
        {
            get { return contentLength; }
            set { contentLength = value; }
        }

        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        /// <summary>
        /// chunk 判定
        /// </summary>
        public string TransferEncoding
        {
            get { return transferEncoding; }
            set { transferEncoding = value; }
        }

        /// <summary>
        /// 获取 HTTP 头
        /// </summary>
        public IList<HttpHeader> Headers
        {
            get { return headers ?? (headers = new List<HttpHeader>()); }
        }
    }
}
