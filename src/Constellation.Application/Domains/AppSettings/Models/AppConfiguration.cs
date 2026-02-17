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
    public MandatoryTrainingConfiguration MandatoryTraining { get; set; }
    public AttachmentsConfiguration Attachments { get; set; }


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

    public class AttachmentsConfiguration
    {
        public string BaseFilePath { get; set; }
        public int MaxDBStoreSize { get; set; }
    }
}
