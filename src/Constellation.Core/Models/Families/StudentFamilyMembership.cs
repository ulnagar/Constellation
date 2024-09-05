#nullable enable
namespace Constellation.Core.Models.Families;

using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.ValueObjects;
using Identifiers;
using Newtonsoft.Json;

public sealed class StudentFamilyMembership
{
    private StudentFamilyMembership() { }

    [JsonConstructor]
    private StudentFamilyMembership(
        StudentId studentId,
        StudentReferenceNumber studentReferenceNumber,
        FamilyId familyId,
        bool isResidentialFamily)
    {
        StudentId = studentId;
        StudentReferenceNumber = studentReferenceNumber; 
        FamilyId = familyId;
        IsResidentialFamily = isResidentialFamily;
    }

    public StudentId StudentId { get; private set; }
    public StudentReferenceNumber StudentReferenceNumber { get; private set;}
    public FamilyId FamilyId { get; private set; }
    public bool IsResidentialFamily { get; internal set; }

    public static StudentFamilyMembership Create(
        StudentId studentId, 
        StudentReferenceNumber srn,
        FamilyId familyId, 
        bool isResidentialFamily)
    {
        return new StudentFamilyMembership(
            studentId,
            srn,
            familyId,
            isResidentialFamily);
    }
}
