using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RateLimitingAlgorithms
{
    public class WebServer
    {
        private TcpListener _tcpListener;
        private IRateLimiter _rateLimiter;


        public WebServer(int port, IRateLimiter rateLimiter)
        {
            _tcpListener = new TcpListener(IPAddress.Any,port);
            _rateLimiter = rateLimiter;
        }

        public async Task StartAsync()
        {
            _tcpListener.Start();
            Console.WriteLine("Server started : " + ((IPEndPoint)_tcpListener.LocalEndpoint).Address + ((IPEndPoint)_tcpListener.LocalEndpoint).Port);
            while (true)
            {
                var client = await _tcpListener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client ,_rateLimiter);
            }
        }
        

        private async Task HandleClientAsync(TcpClient client , IRateLimiter rateLimiter)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                StreamReader reader = new StreamReader(stream , Encoding.ASCII , leaveOpen: true);
                StreamWriter writer = new StreamWriter(stream , Encoding.ASCII , 1024, leaveOpen: true);



                if(!rateLimiter.AllowRequest())
                {
                    string response = "HTTP/1.1 429 Too Many Requests\r\n" +
                                  "Content-Length: 0\r\n\r\n";
                    await writer.WriteAsync(response);
                    await writer.FlushAsync();
                    client.Close();
                    return;
                }

                string requestLine = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(requestLine))
                    return;

                string[] requestParts = requestLine.Split(' ');
                if (requestParts.Length < 3)
                    return;

                string method = requestParts[0];
                string path = requestParts[1];


                if(method == "GET" && path == "/")
                {
                    string response = "HTTP/1.1 200 OK\r\n" +
                                      "Content-Length: 0\r\n\r\n";
                    await writer.WriteAsync(response);
                }
                else
                {
                    string response = "HTTP/1.1 404 Not Found\r\n" +
                                      "Content-Length: 0\r\n\r\n";
                    await writer.WriteAsync(response);
                }

                await writer.FlushAsync();

            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }


    }
}
