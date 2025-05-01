namespace Constellation.Application.Domains.Faculties.Queries.GetFaculty;

using Core.Models.Faculties.Identifiers;

public sealed record FacultyResponse(
    FacultyId FacultyId,
    string Name,
    string Colour);