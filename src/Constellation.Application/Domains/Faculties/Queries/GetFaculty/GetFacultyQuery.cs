namespace Constellation.Application.Domains.Faculties.Queries.GetFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;

public sealed record GetFacultyQuery(
        FacultyId FacultyId)
    : IQuery<FacultyResponse>;