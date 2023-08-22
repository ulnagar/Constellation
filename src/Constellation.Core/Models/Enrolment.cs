using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
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
        public OfferingId OfferingId { get; set; }
        public Offering Offering { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}