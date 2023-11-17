namespace Constellation.Application.Faculties.GetFacultyDetails;

using Abstractions.Messaging;
using Core.Models.Faculty.Identifiers;

public sealed record GetFacultyDetailsQuery(
        FacultyId FacultyId)
    : IQuery<FacultyDetailsResponse>;