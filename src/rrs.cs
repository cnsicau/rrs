using System;
using System.Net;
using System.Threading;
using rrs.Tunnel;

namespace rrs
{
    class rrs
    {
        static void Connect(IPipeline pipeline, bool success, object state)
        {
            if (!success) return;

            pipeline.Input<object>(CompleteInput);
        }

        static void CompleteInput(IPipeline pipeline, IPacket packet, object state)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} receive {(packet as TunnelPacket).Type } {packet.GetType().Name} {packet.Size}B.");

            pipeline.Input<object>(CompleteInput);
        }

        static void OnConnect(IPipeline pipeline, bool success, TunnelPipeline tunnelPipeline)
        {
            var packet = new TunnelPacket(tunnelPipeline);
            packet.Type = TunnelPacketType.Terminate;

            var data = BitConverter.GetBytes(DateTime.Now.Ticks);
            // 两头两尾包含数据
            Array.Copy(data, ((IPacket)packet).Buffer, data.Length);
            Array.Copy(data, 0, ((IPacket)packet).Buffer, Packet.BufferSize - data.Length, data.Length);
            ((IPacket)packet).Relive(Packet.BufferSize);

            tunnelPipeline.Output(packet, CompleteOutput, default(object));
        }

        static void CompleteOutput(IPipeline pipeline, IPacket packet, object state)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} send {(packet as TunnelPacket).Type } {packet.GetType().Name} {packet.Size}B.");
            Thread.Sleep(1000);
            pipeline.Output(packet, CompleteOutput, state);
        }

        static void Main(string[] args)
        {
            Console.Write("run as server(y/n) ? ");

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

            Console.WriteLine("\nAny key to exit.");
            Console.ReadKey(true);
        }
    }
}
