namespace Constellation.Application.Domains.Schools.Queries.GetSchoolDetails;

using Core.Models.Offerings.Identifiers;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;
using Students.Queries.GetCurrentStudentsWithCurrentOfferings;
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
    string Directorate,
    string PrincipalNetwork,
    string EducationalServicesTeam,
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
        Position Role,
        string Note,
        PhoneNumber PhoneNumber,
        EmailAddress EmailAddress);
}