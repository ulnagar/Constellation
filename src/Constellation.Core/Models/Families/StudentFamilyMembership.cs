#nullable enable
namespace Constellation.Core.Models.Families;

using Constellation.Core.Models.Students.Identifiers;
using Identifiers;
using Newtonsoft.Json;

public sealed class StudentFamilyMembership
{
    //private StudentFamilyMembership() { }

    [JsonConstructor]
    private StudentFamilyMembership(
        StudentId studentId,
        FamilyId familyId,
        bool isResidentialFamily)
    {
        StudentId = studentId;
        FamilyId = familyId;
        IsResidentialFamily = isResidentialFamily;
    }

    public StudentId StudentId { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public bool IsResidentialFamily { get; internal set; }

    public static StudentFamilyMembership Create(
        StudentId studentId, 
        FamilyId familyId, 
        bool isResidentialFamily)
    {
        return new StudentFamilyMembership(
            studentId,
            familyId,
            isResidentialFamily);
    }
}
