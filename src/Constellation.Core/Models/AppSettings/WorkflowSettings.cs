namespace Constellation.Core.Models.AppSettings;

using Core.Enums;
using Enums;
using StaffMembers.Identifiers;

public sealed class WorkflowSettings
{
    private readonly List<StaffMemberLink> _members = [];

    private WorkflowSettings() { }

    public WorkflowSettings(
        WorkflowArea position)
    {
        PositionName = position;
    }

    public WorkflowArea PositionName { get; private set; }
    public IReadOnlyList<StaffMemberLink> Members => _members.AsReadOnly();

    public void AddMember(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _members.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.AddGrade(grade);

            return;
        }

        StaffMemberLink newEntry = new StaffMemberLink(staffId, grades);
        _members.Add(newEntry);
    }

    public void RemoveMember(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _members.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.RemoveGrade(grade);

            if (existingEntry.Grades.Count == 0)
                _members.Remove(existingEntry);
        }
    }
}