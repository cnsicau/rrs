using System;
using System.Net;
using Rrs.Ssl;
using Rrs.Tcp;

namespace Rrs
{
    class Program
    {
        static void OnAccept(IPipeline pipeline, bool success, object state)
        {
            var remote = new SslClientPipeline(IPAddress.Parse("47.104.204.209"), 443);
            remote.Connect(OnConnect, pipeline);
        }

        static void OnConnect(IPipeline remote, bool success, IPipeline client)
        {
            new PipelineConnector(remote).Connect(client);
        }

        static void Main(string[] args)
        {
            var server = new TcpPipelineServer(IPAddress.Any, 8088, 512);
            var tunnelServer = new Tunnel.TunnelPipelineServer(server);
            tunnelServer.Run(OnAccept, default(object));
            Console.WriteLine("Any key to exit.");
            Console.ReadKey(true);
        }
    }
}