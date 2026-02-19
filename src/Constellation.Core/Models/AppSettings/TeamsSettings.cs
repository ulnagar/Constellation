namespace Constellation.Core.Models.AppSettings;

using Core.Enums;
using StaffMembers.Identifiers;

public sealed class TeamsSettings
{
    private readonly List<StaffMemberLink> _mandatoryOwners = [];
    private readonly List<StaffMemberLink> _studentTeamOwners = [];
    private readonly List<StaffMemberLink> _studentChannelOwners = [];

    public TeamsSettings() { }


    public List<StaffMemberLink> MandatoryOwners => _mandatoryOwners;
    public List<StaffMemberLink> StudentTeamOwners => _studentTeamOwners;
    public List<StaffMemberLink> StudentChannelOwners => _studentChannelOwners;

    public void AddMandatoryOwner(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _mandatoryOwners.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.AddGrade(grade);

            return;
        }

        StaffMemberLink newEntry = new StaffMemberLink(staffId, grades);
        _mandatoryOwners.Add(newEntry);
    }

    public void RemoveMandatoryOwner(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _mandatoryOwners.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.RemoveGrade(grade);

            if (existingEntry.Grades.Count == 0)
                _mandatoryOwners.Remove(existingEntry);
        }
    }

    public void AddStudentTeamOwner(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _studentTeamOwners.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.AddGrade(grade);

            return;
        }

        StaffMemberLink newEntry = new StaffMemberLink(staffId, grades);
        _studentTeamOwners.Add(newEntry);
    }

    public void RemoveStudentTeamOwner(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _studentTeamOwners.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.RemoveGrade(grade);

            if (existingEntry.Grades.Count == 0)
                _studentTeamOwners.Remove(existingEntry);
        }
    }

    public void AddStudentChannelOwner(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _studentChannelOwners.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.AddGrade(grade);

            return;
        }

        StaffMemberLink newEntry = new StaffMemberLink(staffId, grades);
        _studentChannelOwners.Add(newEntry);
    }

    public void RemoveStudentChannelOwner(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _studentChannelOwners.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.RemoveGrade(grade);

            if (existingEntry.Grades.Count == 0)
                _studentChannelOwners.Remove(existingEntry);
        }
    }
}