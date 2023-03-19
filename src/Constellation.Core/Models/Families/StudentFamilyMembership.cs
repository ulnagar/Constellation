#nullable enable
namespace Constellation.Core.Models.Families;

using Constellation.Core.Models.Identifiers;
using System;

public sealed class StudentFamilyMembership
{
    private StudentFamilyMembership(
        string studentId,
        FamilyId familyId,
        bool isResidentialFamily)
    {
        StudentId = studentId;
        FamilyId = familyId;
        IsResidentialFamily = isResidentialFamily;
    }

    public string StudentId { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public bool IsResidentialFamily { get; internal set; }

    public static StudentFamilyMembership Create(string studentId, FamilyId familyId, bool isResidentialFamily)
    {
        return new StudentFamilyMembership(
            studentId,
            familyId,
            isResidentialFamily);
    }
}
