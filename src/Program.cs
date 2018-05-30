using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace rrs
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SocketPipelineServer(IPAddress.Any, 8811, 50);
            server.Run<object>((pipeline, state) =>
            {
                try
                {
                    Console.WriteLine($"client {pipeline} connected.");
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect("192.168.5.10", 80);

                    var connector = new PipelineConnector(pipeline);

                    connector.Disposed += (s, e) => Console.WriteLine($"client {pipeline} disconnected.");
                    connector.Connect(new SocketPipeline(socket));
                }
                catch (Exception e)
                {
                    using (pipeline)
                    {
                        Console.WriteLine($"client {pipeline} faulted: {e}");
                    }
                }
            });

            Console.WriteLine("Any key to exit.");
            Console.ReadKey(true);
        }
    }
}
