using System;
using System.Collections.Generic;
using System.Text;

namespace Rrs.Http
{
    /// <summary>
    /// HTTP 内容
    /// </summary>
    public interface IHttpBody
    {
        /// <summary>
        /// 读取内容
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        void Read<TState>(ReadCallback<TState> callback, TState state);
    }
}
