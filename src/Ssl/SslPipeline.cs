using Rrs.Tcp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Rrs.Ssl
{
    public class SslPipeline : TcpPipeline
    {
        SslStream stream;
        public SslPipeline(Socket socket) : base(socket) { }

        protected override Stream CreateStream()
        {
            if (stream == null) throw new InvalidOperationException("stream is null");

            return stream;
        }

        public void Authenticate<TState>(X509Certificate certificate, PipelineCallback<TState> callback, TState state)
        {
            try
            {
                stream = new SslStream(base.CreateStream(), false);
                stream.BeginAuthenticateAsServer(certificate, CompleteAuthenticate<TState>, new object[] { callback, state });
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ssl authenticating failed {e}.");
                callback(null, false, state);
                Interrupte();
            }
        }

        void CompleteAuthenticate<TState>(IAsyncResult asr)
        {
            var args = (object[])asr.AsyncState;
            var callback = (PipelineCallback<TState>)args[0];
            var state = (TState)args[1];

            try
            {
                stream.EndAuthenticateAsServer(asr);
                callback(this, true, state);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"ssl authenticated failed {e}.");
                callback(null, false, state);
                Interrupte();
            }
        }
    }
}
