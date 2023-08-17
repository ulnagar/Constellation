namespace Constellation.Application.Interfaces.Configuration;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using System;
using System.Collections.Generic;

public sealed class AppConfiguration
{
    public const string Section = "Constellation:AppSettings";

    public AbsencesConfiguration Absences { get; set; }
    public LessonsConfiguration Lessons { get; set; }
    public MandatoryTrainingConfiguration MandatoryTraining { get; set; }
    public ContactsConfiguration Contacts { get; set; }


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

    public class MandatoryTrainingConfiguration
    {
        public List<string> CoordinatorIds { get; set; }
    }

    public class ContactsConfiguration
    {
        public List<string> CounsellorIds { get; set; }
        public List<string> CareersAdvisorIds { get; set; }
        public Dictionary<Grade, string> LearningSupportIds { get; set; }
    }
}
