namespace Constellation.Core.Models.AppSettings;

using Core.Enums;
using StaffMembers.Identifiers;

public sealed class CanvasSettings
{
    private readonly List<StaffMemberLink> _admins = [];

    private CanvasSettings() { }

    public CanvasSettings(
        bool useGroups,
        bool useSections)
    {
        UseGroups = useGroups;
        UseSections = useSections;
    }

    public bool UseGroups { get; private set; }
    public bool UseSections { get; private set; }
    public List<StaffMemberLink> Admins => _admins;

    public void AddAdmin(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _admins.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.AddGrade(grade);

            return;
        }

        StaffMemberLink newEntry = new(staffId, grades);
        _admins.Add(newEntry);
    }

    public void RemoveAdmin(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _admins.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is null)
            return;

        foreach (Grade grade in grades)
            existingEntry.RemoveGrade(grade);

        if (existingEntry.Grades.Count == 0)
            _admins.Remove(existingEntry);
    }
}