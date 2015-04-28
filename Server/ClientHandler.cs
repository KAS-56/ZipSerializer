using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using DTO;
using Serializer;

namespace Server
{
    class ClientHandler
    {
        readonly Tourist _tourist;
        public ClientHandler(TcpClient client)
        {
            Console.WriteLine("Server accept tcp client");
            using (var stream = new MemoryStream(1024))
            {
                var clientStream = client.GetStream();
                var buffer = new byte[1024];
                int count;
                while ((count = clientStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, count);
                    if (count < 1024)
                    {
                        Console.WriteLine("Server got data");
                        _tourist = (Tourist)BinaryGZipSerializer.Deserialize(stream, typeof(Tourist));
                        Console.WriteLine("Server deserialized data");
                        break;
                    }
                }
                buffer = Encoding.ASCII.GetBytes("ok");
                clientStream.Write(buffer, 0, buffer.Length);
                Console.WriteLine("Server send ok");
            }
            client.Close();
        }

        public Tourist Tourist { get { return _tourist;} }
    } 
}
