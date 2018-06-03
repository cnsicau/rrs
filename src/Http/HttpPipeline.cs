using Rrs.Tcp;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Rrs.Http
{
    public class HttpPipeline : IPipeline
    {
        IPipeline transPipeline;
        HttpRequestReader reader;

        public HttpPipeline(IPipeline transPipeline)
        {
            this.transPipeline = transPipeline;
            transPipeline.Interrupted += OnInterrupted;
            reader = new HttpRequestReader(this);
        }

        private void OnInterrupted(object sender, EventArgs e) { Interrupted?.Invoke(this, e); }

        /// <summary>
        /// 底层传输管道
        /// </summary>
        public IPipeline TransPipeline { get { return transPipeline; } }

        public event EventHandler Interrupted;

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
