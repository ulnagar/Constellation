﻿namespace Constellation.Application.Interfaces.Configuration;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Core.Models.StaffMembers.ValueObjects;
using System.Collections.Generic;

public sealed class AppConfiguration
{
    public const string Section = "Constellation:AppSettings";

    public string DebugLabel { get; set; }

    public AbsencesConfiguration Absences { get; set; }
    public LessonsConfiguration Lessons { get; set; }
    public MandatoryTrainingConfiguration MandatoryTraining { get; set; }
    public ContactsConfiguration Contacts { get; set; }
    public AttachmentsConfiguration Attachments { get; set; }
    public CoversConfiguration Covers { get; set; }
    public WorkFlowConfiguration WorkFlow { get; set; }


    public class AbsencesConfiguration
    {
        public List<AbsenceReason> DiscountedWholeReasons { get; set; }

        public List<AbsenceReason> DiscountedPartialReasons { get; set; }

        //public DateTime AbsenceScanStartDate { get; set; }

        public int PartialLengthThreshold { get; set; }

        //public List<string> ForwardAbsenceEmailsTo { get; set; }

        //public List<string> ForwardTruancyEmailsTo { get; set; }

        public string AbsenceCoordinatorName { get; set; }

        public string AbsenceCoordinatorTitle { get; set; }

        public string AbsenceCoordinatorEmail { get; set; }

        public List<string> SendRollMarkingReportTo { get; set; }
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
        public List<EmployeeId> CounsellorIds { get; set; }
        public List<EmployeeId> CareersAdvisorIds { get; set; }
        public List<EmployeeId> LibrarianIds { get; set; }
        public Dictionary<Grade, List<EmployeeId>> LearningSupportIds { get; set; }
        public Dictionary<Grade, List<EmployeeId>> DeputyPrincipalIds { get; set; }
        public EmployeeId PrincipalId { get; set; }
        public EmployeeId InstructionalLeader { get; set; }
    }

    public class AttachmentsConfiguration
    {
        public string BaseFilePath { get; set; }
        public int MaxDBStoreSize { get; set; }
    }

    public class CoversConfiguration
    {
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string ContactPhone { get; set; }
    }

    public class WorkFlowConfiguration
    {
        public EmployeeId AttendanceReviewer { get; set; }
        public EmployeeId ComplianceReviewer { get; set; }
        public EmployeeId TrainingReviewer { get; set; }
    }
}
