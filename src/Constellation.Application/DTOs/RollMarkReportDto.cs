using System;

namespace Constellation.Application.DTOs
{
    public class RollMarkReportDto
    {
        // Imported from API
        public DateTime Date { get; set; }
        public string Period { get; set; }
        public string ClassName { get; set; }
        public string Teacher { get; set; }
        public string Year { get; set; }
        public string Room { get; set; }
        public bool Submitted { get; set; }

        // Collated from Database
        public int OfferingId { get; set; }
        public bool Covered { get; set; }
        public string CoveredBy { get; set; }
        public string CoverType { get; set; }
        public string HeadTeacher { get; set; }
        public string EmailSentTo { get; set; }
        public string Description { get; set; }
    }
}
