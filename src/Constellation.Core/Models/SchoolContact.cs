using System;
using System.Collections.Generic;

namespace Constellation.Core.Models
{
    public class SchoolContact
    {
        public SchoolContact()
        {
            Assignments = new List<SchoolContactRole>();
            LessonRolls = new List<LessonRoll>();

            IsDeleted = false;
            DateEntered = DateTime.Now;
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateEntered { get; set; }
        public bool SelfRegistered { get; set; }
        public string DisplayName => FirstName + " " + LastName;
        public ICollection<SchoolContactRole> Assignments { get; set; }
        public ICollection<LessonRoll> LessonRolls { get; set; }
    }
}
