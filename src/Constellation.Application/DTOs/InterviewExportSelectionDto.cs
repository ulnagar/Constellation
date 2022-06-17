using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class InterviewExportSelectionDto
    {
        public InterviewExportSelectionDto()
        {
            Grades = new List<int>();
            ClassList = new List<int>();
        }

        public ICollection<int> Grades { get; set; }
        public ICollection<int> ClassList { get; set; }        
        public bool PerFamily { get; set; }
    }
}
