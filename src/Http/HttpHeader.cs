using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Http
{
    /// <summary>
    /// 
    /// </summary>
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

        public string Name { get => name; set => name = value; }

        public string Value { get => value; set => this.value = value; }
    }
}
