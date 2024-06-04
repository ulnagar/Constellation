namespace Constellation.Application.Faculties.GetFaculty;

using Core.Models.Faculties.Identifiers;

public sealed record FacultyResponse(
    FacultyId FacultyId,
    string Name,
    string Colour);