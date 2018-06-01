using Rrs.Tcp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Rrs.Http
{
    class HttpPipeline : IPipeline
    {
        IPipeline transPipeline;
        HttpRequestReader reader;

        public HttpPipeline(IPipeline transPipeline)
        {
            this.transPipeline = transPipeline;
            reader = new HttpRequestReader(transPipeline);
        }

        /// <summary>
        /// 底层传输管道
        /// </summary>
        public IPipeline TransPipeline { get { return transPipeline; } }

        event EventHandler IPipeline.Interrupted
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        void IDisposable.Dispose() { transPipeline.Dispose(); }

        void IPipeline.Input<TState>(IOCompleteCallback<TState> callback, TState state)
        {
            reader.Read(callback, state);
        }

        void IPipeline.Interrupte()
        {
            transPipeline.Interrupte();
        }

        void IPipeline.Output<TState>(IPacket packet, IOCompleteCallback<TState> callback, TState state)
        {
            throw new NotImplementedException();
        }
    }
}
