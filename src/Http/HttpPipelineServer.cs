using System;

namespace Rrs.Http
{
    public class HttpPipelineServer : IPipelineServer
    {
        IPipelineServer server;

        public HttpPipelineServer(IPipelineServer server)
        {
            this.server = server;
            server.Disposed += OnDisposed;
        }

        public event EventHandler Disposed;

        void OnDisposed(object sender, EventArgs e) { Disposed?.Invoke(this, e); }

        public void Dispose() { server.Dispose(); }

        public void Run<TState>(PipelineCallback<TState> accept, TState state = default(TState))
        {
            server.Run(OnAccept<TState>, new object[] { accept, state });
        }

        void OnAccept<TState>(IPipeline pipeline, bool success, object[] args)
        {
            var accept = (PipelineCallback<TState>)args[0];
            var state = (TState)args[1];

            accept(success ? new HttpPipeline(pipeline) : pipeline, success, state);
        }
    }
}
