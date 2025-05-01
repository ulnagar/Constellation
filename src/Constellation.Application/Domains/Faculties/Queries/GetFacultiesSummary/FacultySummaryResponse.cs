namespace Constellation.Application.Domains.Faculties.Queries.GetFacultiesSummary;

using Core.Models.Faculties.Identifiers;

public sealed record FacultySummaryResponse(
    FacultyId FacultyId,
    string Name,
    string Colour,
    int MemberCount);