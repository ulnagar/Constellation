using System;
using System.Collections.Generic;

namespace Constellation.Application.Interfaces.Configuration;

public sealed class AppConfiguration
{
    public const string Section = "Constellation:AppSettings";

    public AbsencesConfiguration Absences { get; set; }
    public LessonsConfiguration Lessons { get; set; }


    public class AbsencesConfiguration
    {
        public List<AbsenceReason> DiscountedWholeReasons { get; set; }

        public List<AbsenceReason> DiscountedPartialReasons { get; set; }

        public DateTime AbsenceScanStartDate { get; set; }

        public int PartialLengthThreshold { get; set; }

        public List<string> ForwardAbsenceEmailsTo { get; set; }

        public List<string> ForwardTruancyEmailsTo { get; set; }

        public string AbsenceCoordinatorName { get; set; }

        public string AbsenceCoordinatorTitle { get; set; }

        public string AbsenceCoordinatorEmail { get; set; }
    }

    public class LessonsConfiguration
    {
        public string CoordinatorEmail { get; set; }
        public string CoordinatorName { get; set; }
        public string CoordinatorTitle { get; set; }
        public List<string> HeadTeacherEmail { get; set; }
    }
}
