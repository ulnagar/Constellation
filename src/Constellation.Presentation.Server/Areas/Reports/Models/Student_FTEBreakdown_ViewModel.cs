using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class Student_FTEBreakdown_ViewModel : BaseViewModel
    {
        public Student_FTEBreakdown_ViewModel()
        {
            Grades = new List<GradeEntry>();
        }

        public ICollection<GradeEntry> Grades { get; set; }
        public int TotalEnrolments { get; set; }
        public decimal TotalEnrolmentFTE { get; set; }


        public class GradeEntry
        {
            public string Grade { get; set; }
            public int Enrolments { get; set; }
            public decimal EnrolmentFTE { get; set; }
        }
    }
}