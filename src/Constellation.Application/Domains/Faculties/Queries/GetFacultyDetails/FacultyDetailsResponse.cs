namespace Constellation.Application.Domains.Faculties.Queries.GetFacultyDetails;

using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using Core.Models.StaffMembers.Identifiers;
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
        StaffId StaffId,
        Name StaffName,
        FacultyMembershipRole Role);
}
