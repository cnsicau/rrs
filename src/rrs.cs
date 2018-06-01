using System;
using System.Net;
using System.Threading;
using Rrs.Tunnel;
using Rrs.Ssl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IO;
using Rrs.Http;
using Rrs.Tcp;

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

        static void OnAcceptHttp(IPipeline pipeline, bool success, object state)
        {
            pipeline.Input((pi, pa, s) =>
            {

            }, state);
        }

        static void CompleteOutput(IPipeline pipeline, IPacket packet, object state)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} send {(packet as TunnelPacket).Type } {packet.GetType().Name} {packet.Size}B.");
            Thread.Sleep(50);
            pipeline.Output(packet, CompleteOutput, state);
        }

        static void Main(string[] args)
        {
            var http = new HttpPipelineServer(new TcpPipelineServer(IPAddress.Any, 8088, 1));
            http.Run<object>(OnAcceptHttp);

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

            Console.WriteLine("\nAny key to exit.");
            Console.ReadKey(true);
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