namespace Constellation.Application.Faculties.GetFacultiesForSelectionList;

using Core.Models.Faculties.Identifiers;

public sealed record FacultySummaryResponse(
    FacultyId FacultyId,
    string Name);