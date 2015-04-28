using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using DTO;
using Serializer;

namespace Client
{
    public static class Client
    {
        public static void Main()
        {
            Console.WriteLine("Client started");
            var tourist = new Tourist()
            {
                FirstName = "Test",
                LastName = "Tester",
                MiddleName = "as",
                BirthDay = new DateTime(1970, 1, 1),
                ForeignPassport =
                    new Passport()
                    {
                        Serial = "123",
                        Number = "456",
                        PassportDate = new DateTime(2010, 1, 1),
                        PassportDateEnd = new DateTime(2020, 1, 1),
                        FirstName = "Passport"
                    },
                NickNames = new string[] {"qwe", "asd", "zxc"},
                TravelCount = 55,
                Profit = 12345m,
                Address =
                    new Address()
                    {
                        Country = "Россия",
                        City = "Москва",
                        Street = "Красная площадь",
                        House = "1",
                        Flat = "1"
                    }
            };

            using (var stream = new MemoryStream(1024))
            {
                BinaryGZipSerializer.Serialize(stream, tourist);
                Console.WriteLine("Client serialized:");
                Console.WriteLine(tourist);
                Console.WriteLine(tourist.Address);
                var client = new TcpClient("localhost", 11000);
                var serverStream = client.GetStream();
                stream.WriteTo(serverStream);
                string response = String.Empty;
                byte[] buffer = new byte[1024];
                int count;
                while ((count = serverStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    response += Encoding.ASCII.GetString(buffer, 0, count);
                } 
                Console.WriteLine("Client send object to server");
                Console.WriteLine("Server say: {0}", response);
            }
        }
    }

}

