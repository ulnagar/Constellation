using System;

namespace Constellation.Infrastructure.Persistence.TrackItContext.Models
{
    public partial class Location
    {
        public int Sequence { get; set; }
        public DateTime Lastmodified { get; set; }
        public string Lastuser { get; set; }
        public int? Group { get; set; }
        public int? Owner { get; set; }
        public int? Ownerperms { get; set; }
        public string Note { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Comments { get; set; }
        public string MainContact { get; set; }
        public string Maincontctphone { get; set; }
        public string TimeZone { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string IntlPostCode { get; set; }
        public string IntlPhone { get; set; }
        public string IntlFax { get; set; }
        public int? SeqPriority { get; set; }
        public short Inactive { get; set; }
        public int? SeqCountry { get; set; }

        public Location()
        {
            Group = 1;
            Inactive = 0;
            Updated();
        }

        public void Updated()
        {
            Lastmodified = DateTime.Now;
            Lastuser = "BENJAMIN.HILLSLEY";
        }
    }
}
