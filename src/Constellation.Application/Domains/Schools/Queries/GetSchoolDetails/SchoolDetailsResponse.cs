namespace Constellation.Application.Domains.Schools.Queries.GetSchoolDetails;

using Core.Models.Offerings.Identifiers;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Tutorials.Identifiers;
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
        StaffId StaffId,
        Name Name,
        Dictionary<string, string> Faculties,
        List<EnrolmentResponse> Offerings);

    public abstract record EnrolmentResponse(
        string Name,
        bool Current);

    public sealed record OfferingEnrolmentResponse(
        OfferingId OfferingId,
        string Name,
        bool Current) 
        : EnrolmentResponse(Name, Current);

    public sealed record TutorialEnrolmentResponse(
        TutorialId TutorialId,
        string Name,
        bool Current)
        : EnrolmentResponse(Name, Current);

    public sealed record SchoolContact(
        SchoolContactId ContactId,
        SchoolContactRoleId RoleId,
        Name Name,
        Position Role,
        string Note,
        PhoneNumber PhoneNumber,
        EmailAddress EmailAddress);
}