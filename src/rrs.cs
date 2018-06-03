using System;
using System.Net;
using Rrs.Tcp;

namespace Rrs
{
    class rrs
    {
        static void OnAccept(IPipeline pipeline, bool success, object state)
        {
            var remote = new TcpClientPipeline(IPAddress.Parse("47.104.204.209"), 80);
            remote.Connect(OnConnect, pipeline);
        }

        static void OnConnect(IPipeline remote, bool success, IPipeline client)
        {
            new PipelineConnector(remote).Connect(client);
        }

        static void Main(string[] args)
        {
            var server = new TcpPipelineServer(IPAddress.Any, 8088, 512);
            server.Run(OnAccept, default(object));
            Console.WriteLine("\nAny key to exit.");
            Console.ReadKey(true);
        }
    }
}