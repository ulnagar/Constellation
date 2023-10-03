using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class InterviewExportSelectionDto
    {
        public InterviewExportSelectionDto()
        {
            Grades = new List<int>();
            ClassList = new List<OfferingId>();
        }

        public ICollection<int> Grades { get; set; }
        public ICollection<OfferingId> ClassList { get; set; }        
        public bool PerFamily { get; set; }
        public bool ResidentialFamilyOnly { get; set; }
    }
}
