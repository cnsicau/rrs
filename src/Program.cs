using System;
using System.Net.Sockets;
using System.Text;

namespace rrs
{
    class Program
    {
        static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("umeqy.com", 17018);

            IPipeline pipeline = new SocketPipeline(socket);
            Action<IPacket, string> callback = null;
            callback = (packet, state) =>
            {
                Console.WriteLine("count: " + packet.Size + " " + Encoding.UTF8.GetString(packet.Buffer, 0, packet.Size));
                packet.Dispose();
                pipeline.Input(callback, state);
            };

            pipeline.Input(callback, string.Empty);

            IPacket req = new SocketPacket((SocketPipeline)pipeline);
            var bytes = Encoding.UTF8.GetBytes("GET / HTTP/1.1\r\nHost: umeqy.com:17018\r\n\r\n");
            Array.Copy(bytes, req.Buffer, bytes.Length);
            ((SocketPacket)req).SetSize(bytes.Length);

            pipeline.Output(req, state => { Console.WriteLine("request completed."); }, string.Empty);

            Console.WriteLine("Any key to exit.");
            Console.ReadKey(true);

            pipeline.Dispose();

            Console.ReadKey(true);
        }
    }
}
