namespace Constellation.Application.Faculties.GetFacultiesSummary;

using Core.Models.Faculty.Identifiers;

public sealed record FacultySummaryResponse(
    FacultyId FacultyId,
    string Name,
    string Colour,
    int MemberCount);