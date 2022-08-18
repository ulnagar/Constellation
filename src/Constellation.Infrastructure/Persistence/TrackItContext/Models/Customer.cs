using System;

namespace Constellation.Infrastructure.Persistence.TrackItContext.Models
{
    public partial class Customer
    {
        public int Sequence { get; set; }
        public DateTime Lastmodified { get; set; }
        public string Lastuser { get; set; }
        public int? Group { get; set; }
        public int? Owner { get; set; }
        public int? Ownerperms { get; set; }
        public string Note { get; set; }
        public string Client { get; set; }
        public string Fname { get; set; }
        public string Mname { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Ext { get; set; }
        public int? Dept { get; set; }
        public int? BillTo { get; set; }
        public string Mail { get; set; }
        public string Bldng { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public int? Location { get; set; }
        public string Emailid { get; set; }
        public string Position { get; set; }
        public string Fax { get; set; }
        public string Picture { get; set; }
        public string TimeZone { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public DateTime? CliCcdt01 { get; set; }
        public DateTime? CliCcdt02 { get; set; }
        public int? CliCcint01 { get; set; }
        public int? CliCcint02 { get; set; }
        public string CliCctxt01 { get; set; }
        public string CliCctxt02 { get; set; }
        public string CliCctxt03 { get; set; }
        public string CliCctxt04 { get; set; }
        public string CliCctxt05 { get; set; }
        public string CliCctxt06 { get; set; }
        public string Sspwd { get; set; }
        public int? SeqPriority { get; set; }
        public short? Usedept { get; set; }
        public short? Uselocation { get; set; }
        public short? Createdfromssd { get; set; }
        public short? Displayclientcomments { get; set; }
        public short Inactive { get; set; }
        public string Winuserid { get; set; }
        public int? SurveyCounter { get; set; }
        public int? NumSurveys { get; set; }
        public DateTime? LastSurveyed { get; set; }
        public int? SeqSurvey { get; set; }
        public short? DoNotSurvey { get; set; }
        public int? SeqStaff { get; set; }
        public int? SeqCountry { get; set; }
        public string Selfserviceaccess { get; set; }
        public short Selfservicelicense { get; set; }
        public short Wiaenabled { get; set; }
        public string Salt { get; set; }
        public string Sid { get; set; }
        public string Logininfo { get; set; }

        public Customer()
        {
            Selfserviceaccess = "Requestor";
            Selfservicelicense = 0;
            Usedept = 0;
            Uselocation = 0;
            Wiaenabled = 0;
            Createdfromssd = 0;
            Displayclientcomments = 0;
            Updated();
        }

        public void Updated()
        {
            Lastmodified = DateTime.Now;
            Lastuser = "BENJAMIN.HILLSLEY";
        }
    }
}
