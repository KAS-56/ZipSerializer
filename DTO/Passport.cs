using System;

namespace DTO
{
    public class Passport
    {
        public string Serial { get; set; }
        public string Number { get; set; }
        public DateTime PassportDate { get; set; }
        public DateTime PassportDateEnd { get; set; }

        public override string ToString()
        {
            return String.Format("{0} № {1} active to {2}", Serial, Number, PassportDateEnd.ToShortDateString());
        }

        public string FirstName;
    }
}
