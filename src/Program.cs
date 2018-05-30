using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace rrs
{
    class Program
    {
        static void OnConnect(IPipeline remote, IPipeline client)
        {
            Console.WriteLine($"remote {remote} connected.");
            var connector = new PipelineConnector(client);
            client.Interrupted += (s, e) => Console.WriteLine($"client {s} disconnected.");
            remote.Interrupted += (s, e) => Console.WriteLine($"remote {s} disconnected.");
            connector.Connect(client);
        }
        static void Main(string[] args)
        {
            var server = new SocketPipelineServer(IPAddress.Any, 8811, 50);
            server.Run<object>((pipeline, state) =>
            {
                try
                {
                    Console.WriteLine($"client {pipeline} connected.");
                    var remote = new ClientSocketPipeline(IPAddress.Parse("192.168.5.10"), 80);
                    remote.Connect(OnConnect, pipeline);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"client {pipeline} faulted: {e}");
                    pipeline.Interrupte();
                }
            });

            Console.WriteLine("Any key to exit.");
            Console.ReadKey(true);
        }
    }
}
