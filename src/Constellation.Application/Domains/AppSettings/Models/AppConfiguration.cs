namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Enums;
using Core.Models.Absences.Enums;
using Core.Models.StaffMembers.ValueObjects;

public sealed class AppConfiguration
{
    public const string Section = "Constellation:AppSettings";

    public string DebugLabel { get; set; }
    public string AdminUser { get; set; } 
    public AbsencesConfiguration Absences { get; set; }
    public LessonsConfiguration Lessons { get; set; }
    public MandatoryTrainingConfiguration MandatoryTraining { get; set; }
    public ContactsConfiguration Contacts { get; set; }
    public AttachmentsConfiguration Attachments { get; set; }
    public CoversConfiguration Covers { get; set; }
    public WorkFlowConfiguration WorkFlow { get; set; }
    public TutorialConfiguration Tutorials { get; set; }


    public class AbsencesConfiguration
    {
        public List<AbsenceReason> DiscountedWholeReasons { get; set; }

        public List<AbsenceReason> DiscountedPartialReasons { get; set; }

        public int PartialLengthThreshold { get; set; }

        public string AbsenceCoordinatorName { get; set; }

        public string AbsenceCoordinatorTitle { get; set; }

        public string AbsenceCoordinatorEmail { get; set; }

        public List<string> SendRollMarkingReportTo { get; set; }
    }

    public class MandatoryTrainingConfiguration
    {
        public List<string> CoordinatorIds { get; set; }
    }

    public class AttachmentsConfiguration
    {
        public string BaseFilePath { get; set; }
        public int MaxDBStoreSize { get; set; }
    }

    public class WorkFlowConfiguration
    {
        public EmployeeId AttendanceReviewer { get; set; }
        public EmployeeId ComplianceReviewer { get; set; }
        public EmployeeId TrainingReviewer { get; set; }
    }

    public sealed class TutorialConfiguration
    {
        public EmployeeId Approver { get; set; }
        public EmployeeId Scheduler { get; set; }
    }
}
