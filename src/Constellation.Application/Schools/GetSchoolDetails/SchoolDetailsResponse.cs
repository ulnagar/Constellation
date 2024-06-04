namespace Constellation.Application.Schools.GetSchoolDetails;

using Constellation.Core.Enums;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;

public sealed record SchoolDetailsResponse(
    string SchoolCode,
    string Name,
    string Address,
    string Town,
    string State,
    string PostCode,
    string PhoneNumber,
    string FaxNumber,
    string EmailAddress,
    bool HeatSchool,
    string Division,
    string PrincipalNetwork,
    string Electorate,
    List<SchoolDetailsResponse.SchoolStaff> Staff,
    List<SchoolDetailsResponse.SchoolStudent> Students,
    List<SchoolDetailsResponse.SchoolContact> Contacts)
{
    public sealed record SchoolStaff(
        string StaffId,
        Name Name,
        Dictionary<string, string> Faculties,
        List<string> Offerings);

    public sealed record SchoolStudent(
        string StudentId,
        Name Name,
        Grade Grade,
        List<string> Offerings);

    public sealed record SchoolContact(
        SchoolContactId ContactId,
        SchoolContactRoleId RoleId,
        Name Name,
        string Role,
        PhoneNumber PhoneNumber,
        EmailAddress EmailAddress);
}