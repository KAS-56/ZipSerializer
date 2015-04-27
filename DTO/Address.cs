using System;

namespace DTO
{
    public struct Address
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Flat { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}, {2}, {3}, {4}", Country, City, Street, House, Flat);
        }
    }
}
