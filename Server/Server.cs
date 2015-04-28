using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class Server
    {
        public static void Main()
        {
            Console.WriteLine("Server started");
            var listener = new TcpListener(IPAddress.Any, 11000);
            listener.Start();
            var clientHandler = new ClientHandler(listener.AcceptTcpClient());
            listener.Stop();
            Console.WriteLine(clientHandler.Tourist);
            Console.WriteLine(clientHandler.Tourist.Address);
        }
    }
}
