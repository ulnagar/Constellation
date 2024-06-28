namespace Constellation.Application.Schools.GetSchoolDetails;

using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.ValueObjects;
using Core.Models.Offerings.Identifiers;
using Students.GetCurrentStudentsWithCurrentOfferings;
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
    List<StudentWithOfferingsResponse> Students,
    List<SchoolDetailsResponse.SchoolContact> Contacts)
{
    public sealed record SchoolStaff(
        string StaffId,
        Name Name,
        Dictionary<string, string> Faculties,
        List<OfferingResponse> Offerings);

    public sealed record OfferingResponse(
        OfferingId Id,
        string Name,
        bool Current);

    public sealed record SchoolContact(
        SchoolContactId ContactId,
        SchoolContactRoleId RoleId,
        Name Name,
        string Role,
        string Note,
        PhoneNumber PhoneNumber,
        EmailAddress EmailAddress);
}