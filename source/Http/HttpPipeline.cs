using System;

namespace Rrs.Http
{
    public class HttpPipeline : IPipeline
    {
        IPipeline transPipeline;
        HttpRequest request;

        public HttpPipeline(IPipeline transPipeline)
        {
            this.transPipeline = transPipeline;
            transPipeline.Interrupted += OnInterrupted;
            request = new HttpRequest(this);
        }

        private void OnInterrupted(object sender, EventArgs e) { Interrupted?.Invoke(this, e); }

        /// <summary>
        /// 底层传输管道
        /// </summary>
        public IPipeline TransPipeline { get { return transPipeline; } }

        public event EventHandler Interrupted;

        void IDisposable.Dispose() { transPipeline.Dispose(); }

        void IPipeline.Input<TState>(IOCallback<TState> callback, TState state)
        {
            request.Construct(callback, state);
        }

        void IPipeline.Interrupte()
        {
            transPipeline.Interrupte();
        }

        void IPipeline.Output<TState>(IPacket packet, IOCallback<TState> callback, TState state)
        {
            throw new NotImplementedException();
        }
    }
}
