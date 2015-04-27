using System;
using System.IO;
using DTO;
using Serializer;

namespace Client
{
    public static class Client
    {
        public static void Main()
        {
            Console.WriteLine("Hello world!!!");
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
                var newTourist = (Tourist)BinaryGZipSerializer.Deserialize(stream, typeof(Tourist));
            }
        }
    }

}

