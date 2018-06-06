using Rrs.Http;
using Rrs.Ssl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Rrs
{
    class tserver
    {
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
        static public void Http()
        {
            Console.Write("\nEnter ssl certificate(ssl.pfx)'s password: ");
            var password = ReadPassword();
            var certificateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ssl.pfx");
            var certificate = new X509Certificate2(certificateFile, password, X509KeyStorageFlags.DefaultKeySet);
            var server = new SslPipelineServer(certificate, IPAddress.Any, 443, 50);
            var http = new HttpPipelineServer(server);
            http.Run<object>(Connect, null);
        }

        static void Connect(IPipeline pipeline, bool success, object state)
        {
            if (success)
                pipeline.Input<object>(ProcessRequest, null);
        }

        static void ProcessRequest(IPipeline pipeline, IPacket packet, object state)
        {
            var request = (HttpRequest)packet;
            var str = ("HTTP/1.1 200 OK\r\n"
+ "Transfer-Encoding: chunked\r\n"
+ "\r\n"
+ request.Uri.Length.ToString("X") + "\r\n"
+ request.Uri + "\r\n"
+ "0\r\n"
+ "\r\n");
            var bytes = Encoding.UTF8.GetBytes(str);
            var response = new BufferPacket(pipeline, bytes);
            response.SetBufferSize(bytes.Length);

            ((HttpPipeline)pipeline).TransPipeline.Output<object>(response
                , (a, b, c) => pipeline.Input<object>(ProcessRequest, null), null);
        }
    }
}
