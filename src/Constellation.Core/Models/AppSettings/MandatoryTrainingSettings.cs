namespace Constellation.Core.Models.AppSettings;

using Core.Enums;
using StaffMembers.Identifiers;

public sealed class MandatoryTrainingSettings
{
    private readonly List<StaffMemberLink> _contacts = [];

    public MandatoryTrainingSettings() { }

    public List<StaffMemberLink> Contacts => _contacts;

    public void AddContact(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _contacts.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.AddGrade(grade);

            return;
        }

        StaffMemberLink newEntry = new(staffId, grades);
        _contacts.Add(newEntry);
    }

    public void RemoveContact(StaffId staffId, List<Grade> grades)
    {
        StaffMemberLink? existingEntry = _contacts.FirstOrDefault(entry => entry.StaffId == staffId);

        if (existingEntry is not null)
        {
            foreach (Grade grade in grades)
                existingEntry.RemoveGrade(grade);

            if (existingEntry.Grades.Count == 0)
                _contacts.Remove(existingEntry);
        }
    }
}