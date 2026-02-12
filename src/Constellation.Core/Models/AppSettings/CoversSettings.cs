namespace Constellation.Core.Models.AppSettings;

using Constellation.Core.Models.StaffMembers.Identifiers;
using Core.Enums;
using Newtonsoft.Json;

public sealed class CoversSettings
{
    private readonly List<StaffMemberLink> _supervisor = [];

    private CoversSettings() { }

    [JsonConstructor]
    private CoversSettings(
        IReadOnlyList<StaffMemberLink> supervisor)
    {
        _supervisor = supervisor.ToList();
    }

    public CoversSettings(
        string name,
        string title,
        string phone)
    {
        ContactName = name;
        ContactTitle = title;
        ContactPhone = phone;
    }

    public string ContactName { get; private set; } = string.Empty;
    public string ContactTitle { get; private set; } = string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
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