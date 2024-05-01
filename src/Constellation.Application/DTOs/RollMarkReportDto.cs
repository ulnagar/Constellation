using System;
using System.Collections.Generic;

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
    }

    public class RollMarkingEmailDto 
    {
        public string RollInformation { get; set; }
        public Dictionary<string, string> Teachers { get; set; } = new();
        public Dictionary<string, string> HeadTeachers { get; set; } = new();
        public string Faculty { get; set; }
        public List<string> Notes { get; set; } = new();
        public string TeacherName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
    }

}
