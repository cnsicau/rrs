using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Http
{
    abstract public class HttpPacket : Packet
    {
        private readonly IList<HttpHeader> headers = new List<HttpHeader>(4);

        public HttpPacket(IPipeline source) : base(source)
        {
        }

        public IList<HttpHeader> Headers { get { return headers; } }
    }
}
