using System;
using System.Collections.Generic;
using Constellation.Core.Models.Covers;

namespace Constellation.Core.Models
{
    public class ClassworkNotification
    {
        public ClassworkNotification()
        {
            Absences = new List<Absence>();
            Teachers = new List<Staff>();
        }

        public Guid Id { get; set; }
        public string Description { get; set; }
        public Staff CompletedBy { get; set; }
        public string StaffId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public ICollection<Absence> Absences { get; set; }
        public ICollection<Staff> Teachers { get; set; }
        public CourseOffering Offering { get; set; }
        public int OfferingId { get; set; }
        public DateTime AbsenceDate { get; set; }
        public bool IsCovered { get; set; }
    }
}
