namespace Constellation.Application.Schools.GetSchoolForEdit;

public sealed record SchoolEditResponse(
    string SchoolCode,
    string Name,
    string Address,
    string Town,
    string State,
    string PostCode,
    string PhoneNumber,
    string FaxNumber,
    string EmailAddress,
    string Division,
    bool HeatSchool,
    string Electorate,
    string PrincipalNetwork,
    string TimetableApplication,
    string RollCallGroup);
