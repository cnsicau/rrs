using System;
using System.Net;

namespace rrs
{
    class Program
    {
        static void Connect(IPipeline pipeline, bool success, object state)
        {
            if (!success) return;

            pipeline.Input<object>(Echo);
        }

        static void Echo(IPipeline pipeline, IPacket packet, object state)
        {
            pipeline.Output<object>(packet, CompleteEcho);
        }

        static void CompleteEcho(IPipeline pipeline, IPacket packet, object state)
        {
            packet.Dispose();
            pipeline.Input<object>(Echo);
        }

        static void OnConnect(IPipeline pipeline, bool success, TunnelPipeline tunnelPipeline)
        {
            var packet = new TunnelPacket(tunnelPipeline);
            packet.Type = TunnelPacketType.Ping;
            var data = BitConverter.GetBytes(DateTime.Now.Ticks);
            Array.Copy(data, ((IPacket)packet).Buffer, data.Length);
            ((IPacket)packet).SetSize(data.Length);

            tunnelPipeline.Output(packet, (a, b, c) => { }, default(object));
        }
        static void Main(string[] args)
        {
            Console.WriteLine("run as server(y/n) ? ");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                var server = new SocketPipelineServer(IPAddress.Any, 8811, 50);
                var tunnelServer = new TunnelPipelineServer(server);
                tunnelServer.Run<object>(Connect);
            }
            else
            {
                var client = new ClientSocketPipeline(IPAddress.Loopback, 8811);
                var tunnelClient = new TunnelPipeline(client);
                client.Connect(OnConnect, tunnelClient);
            }
            Console.WriteLine("Any key to exit.");
            Console.ReadKey(true);
        }
    }
}
