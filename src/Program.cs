using System;
using System.Net;

namespace rrs
{
    class Program
    {
        static void OnConnect(IPipeline remote, IPipeline client)
        {
            Console.WriteLine($"remote {remote} connected.");

            remote.Interrupted += Interrupted;
            client.Interrupted += Interrupted;

            new PipelineConnector(remote).Connect(client);
        }

        static void Interrupted(object sender, EventArgs e)
        {
            Console.WriteLine($"connection {sender} disconnected.");
        }

        static void Accept(IPipeline pipeline, object state)
        {
            Console.WriteLine($"client {pipeline} connected.");
            var remote = new SslClientSocketPipeline(IPAddress.Parse("47.104.204.209"), 443);
            remote.Connect(OnConnect, pipeline);
        }

        static void Main(string[] args)
        {
            var server = new SocketPipelineServer(IPAddress.Any, 8811, 50);
            server.Run<object>(Accept);
            Console.WriteLine("Any key to exit.");
            Console.ReadKey(true);
        }
    }
}
