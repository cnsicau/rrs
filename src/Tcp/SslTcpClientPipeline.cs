using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Rrs.Tcp
{
    public class SslTcpClientPipeline : TcpClientPipeline
    {
        private SslStream sslStream;

        public SslTcpClientPipeline(IPAddress address, int port) : base(address, port) { }

        protected override Stream OnCreateStream()
        {
            if (sslStream == null)
                throw new InvalidOperationException("connection is not connected.");

            return sslStream; ;
        }

        protected override void OnConnected<TState>(PipelineCallback<TState> callback, TState state = default(TState))
        {
            try
            {
                sslStream = new SslStream(base.OnCreateStream(), false, CertificateValidation);
                sslStream.BeginAuthenticateAsClient($"https://{Socket.RemoteEndPoint}", OnAuthenticated<TState>, new object[] { callback, state });
            }
            catch (Exception e)
            {
                callback(this, false, state);
                Trace.WriteLine($"ssl authenticated failed {e}.");
                Interrupte();
            }
        }

        bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; }

        void OnAuthenticated<TState>(IAsyncResult asr)
        {
            var args = (object[])asr.AsyncState;
            try
            {
                sslStream.EndAuthenticateAsClient(asr);
                base.OnConnected((PipelineCallback<TState>)args[0], (TState)args[1]);
            }
            catch (Exception e)
            {
                ((PipelineCallback<TState>)args[0])(this, false, (TState)args[1]);
                Trace.WriteLine($"ssl authenticated failed {e}.");
                Interrupte();
            }
        }
    }
}
