using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace rrs
{
    public class SslClientSocketPipeline : ClientSocketPipeline
    {
        private SslStream sslStream;

        public SslClientSocketPipeline(IPAddress address, int port) : base(address, port) { }

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
                Trace.WriteLine($"ssl authenticated failed {e}.");
                Interrupte();
            }
        }

        bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; }


        void OnAuthenticated<TState>(IAsyncResult asr)
        {
            try
            {
                sslStream.EndAuthenticateAsClient(asr);
                var args = (object[])asr.AsyncState;
                base.OnConnected((PipelineCallback<TState>)args[0], (TState)args[1]);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ssl authenticated failed {e}.");
                Interrupte();
            }
        }
    }
}
