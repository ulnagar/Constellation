namespace Constellation.Application.Domains.Schools.Queries.GetSchoolForEdit;

using Core.Models.Identifiers;

public sealed record SchoolEditResponse(
    SchoolCode SchoolCode,
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
