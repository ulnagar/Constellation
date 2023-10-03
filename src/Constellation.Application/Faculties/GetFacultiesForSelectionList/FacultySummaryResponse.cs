namespace Constellation.Application.Faculties.GetFacultiesForSelectionList;

using System;

public sealed record FacultySummaryResponse(
    Guid FacultyId,
    string Name);