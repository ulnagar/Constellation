using System.Collections.Generic;

namespace Constellation.Core.Models
{
    public class StudentFamily
    {
        public StudentFamily()
        {
            Students = new List<Student>();
            Parent1 = new Parent();
            Parent2 = new Parent();
            Address = new MailingAddress();
        }

        public string Id { get; set; }
        public ICollection<Student> Students { get; set; }
        public Parent Parent1 { get; set; }
        public Parent Parent2 { get; set; }
        public MailingAddress Address { get; set; }
        public string EmailAddress { get; set; }

        public class Parent
        {
            public string Title { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MobileNumber { get; set; }
            public string EmailAddress { get; set; }
        }

        public class MailingAddress
        {
            public string Title { get; set; }
            public string Line1 { get; set; }
            public string Line2 { get; set; }
            public string Town { get; set; }
            public string PostCode { get; set; }
        }
    }
}
