using System;

namespace DTO
{
    public class Tourist
    {
        public Tourist(string name)
        {
            FirstName = name;
        }

        public Tourist()
        {
        }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public Passport LocalPassport { get; set; }
        public Passport ForeignPassport { get; set; }
        public DateTime BirthDay { get; set; }
        public int TravelCount { get; set; }
        public decimal Profit { get; set; }
        public Address Address { get; set; }

        public override string ToString()
        {
            return String.Format("Tourist: {0} {1}{2}, Birthday: {3}, Foreign passport: {4}",
                FirstName, String.IsNullOrWhiteSpace(MiddleName) ? String.Empty : String.Format("{0} ", MiddleName),
                LastName, BirthDay.ToShortDateString(), ForeignPassport);
        }

        private int _age = 100500;

        public string[] NickNames { get; set; }
    }
}
