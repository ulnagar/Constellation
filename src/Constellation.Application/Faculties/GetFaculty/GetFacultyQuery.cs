namespace Constellation.Application.Faculties.GetFaculty;

using Abstractions.Messaging;
using Core.Models.Faculty.Identifiers;

public sealed record GetFacultyQuery(
        FacultyId FacultyId)
    : IQuery<FacultyResponse>;