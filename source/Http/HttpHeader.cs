using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Rrs.Http
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("{name,nq}: {value,nq}")]
    public class HttpHeader
    {
        private string name;
        private string value;

        public HttpHeader() { }

        public HttpHeader(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
