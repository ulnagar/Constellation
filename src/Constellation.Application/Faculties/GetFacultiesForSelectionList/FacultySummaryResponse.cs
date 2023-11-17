namespace Constellation.Application.Faculties.GetFacultiesForSelectionList;

using Core.Models.Faculty.Identifiers;

public sealed record FacultySummaryResponse(
    FacultyId FacultyId,
    string Name);