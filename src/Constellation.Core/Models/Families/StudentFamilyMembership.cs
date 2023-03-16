#nullable enable
namespace Constellation.Core.Models.Families;

using System;

public sealed class StudentFamilyMembership
{
    private StudentFamilyMembership(
        string studentId,
        Guid familyId,
        bool isResidentialFamily)
    {
        StudentId = studentId;
        FamilyId = familyId;
        IsResidentialFamily = isResidentialFamily;
    }

    public string StudentId { get; private set; }
    public Guid FamilyId { get; private set; }
    public bool IsResidentialFamily { get; internal set; }

    public static StudentFamilyMembership Create(string studentId, Guid familyId, bool isResidentialFamily)
    {
        return new StudentFamilyMembership(
            studentId,
            familyId,
            isResidentialFamily);
    }
}
