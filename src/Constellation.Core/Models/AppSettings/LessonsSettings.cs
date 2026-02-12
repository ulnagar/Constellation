namespace Constellation.Core.Models.AppSettings;

using Core.Enums;
using StaffMembers.Identifiers;

public sealed class LessonsSettings
{
    private readonly List<StaffMemberLink> _supervisor = [];

    private LessonsSettings() { }

    public LessonsSettings(
        string name,
        string title,
        string email)
    {
        CoordinatorName = name;
        CoordinatorTitle = title;
        CoordinatorEmail = email;
    }

    public string CoordinatorEmail { get; set; } = string.Empty;
    public string CoordinatorName { get; set; } = string.Empty;
    public string CoordinatorTitle { get; set; } = string.Empty;
    public List<StaffMemberLink> Supervisor => _supervisor;

    public void AddSupervisor(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _supervisor.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.AddGrade(grade);

            return;
        }

        StaffMemberLink newEntry = new StaffMemberLink(staffId, grades);
        _supervisor.Add(newEntry);
    }

    public void RemoveSupervisor(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _supervisor.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.RemoveGrade(grade);

            if (existingEntry.Grades.Count == 0)
                _supervisor.Remove(existingEntry);
        }
    }
}