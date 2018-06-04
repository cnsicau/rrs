using Rrs.Tcp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Rrs.Ssl
{
    public class SslClientPipeline : TcpClientPipeline
    {
        private readonly string host;
        private SslStream stream;

        public SslClientPipeline(IPAddress address, int port, string host = null) : base(address, port)
        {
            this.host = host ?? address.ToString();
        }

        protected override Stream Stream { get { return stream; } }

        protected override void OnConnected<TState>(ConnectCallback<TState> callback, TState state = default(TState))
        {
            try
            {
                stream = new SslStream(base.Stream, false, CertificateValidation);
                stream.BeginAuthenticateAsClient($"https://{host}", OnAuthenticated<TState>, new object[] { callback, state });
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ssl authenticating failed {e}.");
                callback(this, false, state);
                Interrupte();
            }
        }

        bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; }

        void OnAuthenticated<TState>(IAsyncResult asr)
        {
            var args = (object[])asr.AsyncState;
            try
            {
                stream.EndAuthenticateAsClient(asr);
                base.OnConnected((ConnectCallback<TState>)args[0], (TState)args[1]);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ssl authenticated failed {e}.");
                ((ConnectCallback<TState>)args[0])(this, false, (TState)args[1]);
                Interrupte();
            }
        }
    }
}
