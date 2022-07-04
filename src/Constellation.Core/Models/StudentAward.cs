using System;

namespace Constellation.Core.Models
{
    public class StudentAward
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public virtual Student Student { get; set; }
        public DateTime AwardedOn { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
    }
}
