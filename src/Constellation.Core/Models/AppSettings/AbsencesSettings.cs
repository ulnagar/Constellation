namespace Constellation.Core.Models.AppSettings;

using Absences.Enums;
using Core.Enums;
using StaffMembers.Identifiers;

public sealed class AbsencesSettings
{
    private readonly List<AbsenceReason> _ignoredWholeReasons = [];
    private readonly List<AbsenceReason> _ignoredPartialReasons = [];
    private readonly List<StaffMemberLink> _rollMarkingReportRecipients = [];

    private AbsencesSettings() { }

    public AbsencesSettings(
        int partialLengthThreshold,
        string contactName,
        string contactTitle,
        string contactEmail)
    {
        PartialLengthThreshold = partialLengthThreshold;
        ContactName = contactName;
        ContactTitle = contactTitle;
        ContactEmail = contactEmail;
    }

    public int PartialLengthThreshold { get; private set; }
    public string ContactName { get; private set; } = string.Empty;
    public string ContactTitle { get; private set; } = string.Empty;
    public string ContactEmail { get; private set; } = string.Empty;

    public List<AbsenceReason> DiscountedWholeReasons => _ignoredWholeReasons;
    public List<AbsenceReason> DiscountedPartialReasons => _ignoredPartialReasons;
    public List<StaffMemberLink> RollMarkingReportRecipients => _rollMarkingReportRecipients;

    public void AddWholeReason(AbsenceReason reason)
    {
        bool existingEntry = _ignoredWholeReasons.Contains(reason);

        if (existingEntry)
            return;

        _ignoredWholeReasons.Add(reason);
    }

    public void RemoveWholeReason(AbsenceReason reason) =>
        _ignoredWholeReasons.Remove(reason);

    public void AddPartialReason(AbsenceReason reason)
    {
        bool existingEntry = _ignoredPartialReasons.Contains(reason);

        if (existingEntry)
            return;

        _ignoredPartialReasons.Add(reason);
    }

    public void RemovePartialReason(AbsenceReason reason) =>
        _ignoredPartialReasons.Remove(reason);

    public void AddReportRecipient(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _rollMarkingReportRecipients.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.AddGrade(grade);

            return;
        }

        StaffMemberLink newEntry = new(staffId, grades);
        _rollMarkingReportRecipients.Add(newEntry);
    }

    public void RemoveReportRecipient(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _rollMarkingReportRecipients.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.RemoveGrade(grade);

            if (existingEntry.Grades.Count == 0)
                _rollMarkingReportRecipients.Remove(existingEntry);
        }
    }
}