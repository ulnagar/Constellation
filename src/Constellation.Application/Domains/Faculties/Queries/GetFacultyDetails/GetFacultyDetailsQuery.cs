namespace Constellation.Application.Domains.Faculties.Queries.GetFacultyDetails;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;

public sealed record GetFacultyDetailsQuery(
        FacultyId FacultyId)
    : IQuery<FacultyDetailsResponse>;