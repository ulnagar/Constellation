using System;

namespace Constellation.Core.Models
{
    public class SchoolContactRole
    {
        public SchoolContactRole()
        {
            IsDeleted = false;
            DateEntered = DateTime.Now;
        }

        public const string SciencePrac = "Science Practical Teacher";
        public const string Coordinator = "Aurora College Coordinator";
        public const string Principal = "Principal";

        public int Id { get; set; }
        public int SchoolContactId { get; set; }
        public SchoolContact SchoolContact { get; set; }
        public string Role { get; set; }
        public string SchoolCode { get; set; }
        public virtual School School { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateEntered { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}