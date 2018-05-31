using Rrs.Tcp;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Rrs.Ssl
{
    /// <summary>
    /// SSL 服务端
    /// </summary>
    public class SslPipelineServer : TcpPipelineServer
    {
        private readonly X509Certificate certificate;

        public SslPipelineServer(X509Certificate certificate, IPAddress address, int port, int backlog) : base(address, port, backlog)
        {
            this.certificate = certificate;
        }

        protected override void OnCreatePipeline<TState>(Socket socket, PipelineCallback<TState> callback, TState state)
        {
            var pipeline = new SslPipeline(socket);
            pipeline.Authenticate(certificate, callback, state);
        }

        /// <summary>
        /// 服务端证书
        /// </summary>
        public X509Certificate Certificate { get { return certificate; } }
    }
}
