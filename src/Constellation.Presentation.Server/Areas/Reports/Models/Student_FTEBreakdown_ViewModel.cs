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
        public int TotalMaleEnrolments { get; set; }
        public decimal TotalMaleEnrolmentFTE { get; set; }
        public int TotalFemaleEnrolments { get; set; }
        public decimal TotalFemaleEnrolmentFTE { get; set; }
        public int TotalEnrolments { get; set; }
        public decimal TotalEnrolmentFTE { get; set; }


        public class GradeEntry
        {
            public string Grade { get; set; }
            public int MaleEnrolments { get; set; }
            public decimal MaleEnrolmentFTE { get; set; }
            public int FemaleEnrolments { get; set; }
            public decimal FemaleEnrolmentFTE { get; set; }

            public int TotalEnrolments => MaleEnrolments + FemaleEnrolments;
            public decimal TotalEnrolmentFTE => MaleEnrolmentFTE + FemaleEnrolmentFTE;
        }
    }
}