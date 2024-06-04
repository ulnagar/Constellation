namespace Constellation.Application.Faculties.GetFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;

public sealed record GetFacultyQuery(
        FacultyId FacultyId)
    : IQuery<FacultyResponse>;