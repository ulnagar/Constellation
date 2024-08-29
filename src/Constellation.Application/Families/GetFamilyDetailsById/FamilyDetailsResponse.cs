namespace Constellation.Application.Families.GetFamilyDetailsById;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record FamilyDetailsResponse(
    FamilyId FamilyId,
    string FamilyTitle,
    string AddressLine1,
    string AddressLine2,
    string AddressTown,
    string AddressPostCode,
    string FamilyEmail,
    List<FamilyDetailsResponse.ParentResponse> Parents,
    List<FamilyDetailsResponse.StudentResponse> Students,
    bool IsResidential)
{
    public sealed record ParentResponse(
        ParentId ParentId,
        Name Name,
        EmailAddress EmailAddress,
        PhoneNumber? PhoneNumber);

    public sealed record StudentResponse(
        StudentId StudentId,
        Name Name,
        EmailAddress EmailAddress,
        string SchoolCode,
        string SchoolName,
        Grade CurrentGrade);
};