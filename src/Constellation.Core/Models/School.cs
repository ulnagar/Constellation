using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.Students;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Core.Models
{
    public class School
    {
        public School()
        {
            StaffAssignments = new List<SchoolContactRole>();
            Students = new List<Student>();
            Staff = new List<Staff>();
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Division { get; set; }
        public bool HeatSchool { get; set; }
        public string Electorate { get; set; }
        public string PrincipalNetwork { get; set; }
        public string TimetableApplication { get; set; }
        public string RollCallGroup { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Website { get; set; }
        public bool HasStudents => (Students.Any(students => !students.IsDeleted));
        public bool HasStaff => (Staff.Any(staff => !staff.IsDeleted));
        public ICollection<SchoolContactRole> StaffAssignments { get; set; }
        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Staff> Staff { get; set; }
    }
}
