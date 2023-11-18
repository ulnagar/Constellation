namespace Constellation.Application.Faculties.GetFaculty;

using Core.Models.Faculty.Identifiers;

public sealed record FacultyResponse(
    FacultyId FacultyId,
    string Name,
    string Colour);