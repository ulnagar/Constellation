namespace Constellation.Core.Models.AppSettings;

using Core.Enums;
using StaffMembers.Identifiers;

public sealed class StaffMemberLink
{
    private readonly List<Grade> _grades = [];

    public StaffMemberLink(
        StaffId staffId,
        List<Grade> grades)
    {
        StaffId = staffId;

        foreach (Grade grade in grades)
            AddGrade(grade);
    }

    public StaffId StaffId { get; private set; }
    public List<Grade> Grades => _grades;

    public void AddGrade(Grade grade) => _grades.Add(grade);
    public void RemoveGrade(Grade grade) => _grades.Remove(grade);
}