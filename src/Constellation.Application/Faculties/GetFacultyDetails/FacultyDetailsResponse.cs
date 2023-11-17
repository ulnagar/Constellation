namespace Constellation.Application.Faculties.GetFacultyDetails;

using Core.Models.Faculty.Identifiers;
using Core.Models.Faculty.ValueObjects;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record FacultyDetailsResponse(
    FacultyId FacultyId,
    string Name,
    string Colour,
    List<FacultyDetailsResponse.MemberEntry> Members)
{
    public sealed record MemberEntry(
        FacultyMembershipId MembershipId,
        string StaffId,
        Name StaffName,
        FacultyMembershipRole Role);
}
