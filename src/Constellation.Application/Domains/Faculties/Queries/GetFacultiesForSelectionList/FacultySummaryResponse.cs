namespace Constellation.Application.Domains.Faculties.Queries.GetFacultiesForSelectionList;

using Core.Models.Faculties.Identifiers;

public sealed record FacultySummaryResponse(
    FacultyId FacultyId,
    string Name);