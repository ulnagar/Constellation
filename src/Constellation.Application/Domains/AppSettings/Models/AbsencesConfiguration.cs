namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Enums;
using Core.Models.Absences.Enums;
using Core.Models.AppSettings;
using Core.Models.StaffMembers;

public sealed record AbsencesConfiguration
{
    public AbsencesConfiguration(
        AbsencesSettings settings,
        Dictionary<StaffMember, List<Grade>> contacts)
    {
        PartialLengthThreshold = settings.PartialLengthThreshold;
        ContactName = settings.ContactName;
        ContactTitle = settings.ContactTitle;
        ContactEmail = settings.ContactEmail;
        DiscountedPartialReasons = settings.DiscountedPartialReasons;
        DiscountedWholeReasons = settings.DiscountedWholeReasons;
        RollMarkingReportRecipients = contacts;
    }

    public AbsencesConfiguration(
        int partialLengthThreshold,
        string name,
        string title,
        string email,
        List<AbsenceReason> partialReasons,
        List<AbsenceReason> wholeReasons,
        Dictionary<StaffMember, List<Grade>> contacts)
    {
        PartialLengthThreshold = partialLengthThreshold;
        ContactName = name;
        ContactTitle = title;
        ContactEmail = email;

        DiscountedPartialReasons = partialReasons;
        DiscountedWholeReasons = wholeReasons;
        RollMarkingReportRecipients = contacts;
    }
    public int PartialLengthThreshold { get; init; }
    public string ContactName { get; init; }
    public string ContactTitle { get; init; }
    public string ContactEmail { get; init; }

    public IReadOnlyList<AbsenceReason> DiscountedWholeReasons { get; }
    public IReadOnlyList<AbsenceReason> DiscountedPartialReasons { get; }
    public IReadOnlyDictionary<StaffMember, List<Grade>> RollMarkingReportRecipients { get; }
}