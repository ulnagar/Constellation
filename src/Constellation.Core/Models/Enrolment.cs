using Constellation.Core.Models.Subjects;
using System;

namespace Constellation.Core.Models
{
    public class Enrolment
    {
        public Enrolment()
        {
            IsDeleted = false;
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }
        public string StudentId { get; set; }
        public Student Student { get; set; }
        public int OfferingId { get; set; }
        public Offering Offering { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}