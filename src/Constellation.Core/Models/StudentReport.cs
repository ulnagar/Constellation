using System;

namespace Constellation.Core.Models
{
    public class StudentReport
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public Student Student { get; set; }
        public string PublishId { get; set; }
        public string Year { get; set; }
        public string ReportingPeriod { get; set; }
    }
}
