using System;
using System.Net;
using System.Threading;
using Rrs.Tunnel;
using Rrs.Ssl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;

namespace Rrs
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
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} receive { (packet as TunnelPacket)?.Type } {packet?.GetType().Name}.");
            packet.Read(CompleteRead, pipeline);
        }

        static void CompleteRead(PacketData data, IPipeline pipeline)
        {
            if (data.Completed)
            {
                data.Packet.Dispose();
                pipeline.Input<object>(CompleteInput);
            }
            else
            {
                Console.Write($"{DateTime.Now:HH:mm:ss.fff} read {data.Size}B:\t");
                for (int i = 0; i < data.Size && i < 10; i++)
                {
                    Console.Write("{0, 3:x2}", data.Buffer[i]);
                }
                Console.WriteLine();
                data.Packet.Read(CompleteRead, pipeline);
            }
        }

        static void OnConnect(IPipeline pipeline, bool success, TunnelPipeline tunnelPipeline)
        {
            var packet = new BufferPacket(pipeline, new byte[] { 1, 2, 3 });
            packet.SetBufferSize(3);
            tunnelPipeline.Output(packet, CompleteOutput, default(object));
        }

        static void CompleteOutput(IPipeline pipeline, IPacket packet, object state)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} send {(packet as TunnelPacket)?.Type.ToString() ?? "Data" } {packet.GetType().Name}..");
            Thread.Sleep(1000);
            ((BufferPacket)packet).SetBufferSize(3);
            pipeline.Output(packet, CompleteOutput, state);
        }

        static void Main(string[] args)
        {
            Console.Write("run as server(y/n) ? ");

            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.Write("\nEnter ssl certificate password: ");
                var password = ReadPassword();
                var certificateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ssl.pfx");
                var certificate = new X509Certificate2(certificateFile, password, X509KeyStorageFlags.DefaultKeySet);
                var server = new SslPipelineServer(certificate, IPAddress.Any, 8811, 50);
                var tunnelServer = new TunnelPipelineServer(server);
                tunnelServer.Run<object>(Connect);
            }
            else
            {
                var client = new SslClientPipeline(IPAddress.Loopback, 8811);
                var tunnelClient = new TunnelPipeline(client);
                client.Connect(OnConnect, tunnelClient);
            }

            Console.WriteLine("\npress ESC to exit.");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
        }

        static string ReadPassword()
        {
            var password = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    Environment.Exit(1);
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0) password.Length--;
                }
                else
                {
                    password.Append(key.KeyChar);
                }
            }
            return password.ToString();
        }
    }
}