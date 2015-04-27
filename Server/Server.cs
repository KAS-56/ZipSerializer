using System;
using System.IO;
using DTO;
using Serializer;

namespace Server
{
    class Server
    {
        public static void Main()
        {
            using (var stream = new MemoryStream(1024))
            {
                var newTourist = (Tourist)BinaryGZipSerializer.Deserialize(stream, typeof(Tourist));
                Console.WriteLine(newTourist); 
                Console.WriteLine(newTourist.Address);
            }
        }
    }
}
