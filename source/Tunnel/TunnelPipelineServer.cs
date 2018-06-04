using System;

namespace Rrs.Tunnel
{
    public class TunnelPipelineServer : IPipelineServer
    {
        private readonly IPipelineServer server;

        public TunnelPipelineServer(IPipelineServer server)
        {
            this.server = server;
            server.Disposed += OnDisposed;
        }

        void OnDisposed(object sender, EventArgs e) { Disposed?.Invoke(this, e); }

        public event EventHandler Disposed;

        public void Dispose() { server.Dispose(); }

        public void Run<TState>(ConnectCallback<TState> accept, TState state = default(TState))
        {
            server.Run(OnAccept<TState>, new object[] { accept, state });
        }

        void OnAccept<TState>(IPipeline pipeline, bool success, object[] args)
        {
            var accept = (ConnectCallback<TState>)args[0];
            var state = (TState)args[1];

            accept(success ? new TunnelPipeline(pipeline) : pipeline, success, state);
        }
    }
}
