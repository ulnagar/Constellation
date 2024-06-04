namespace Constellation.Application.Faculties.GetFacultyManagers;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Faculties.Identifiers;
using System.Collections.Generic;

public sealed record GetFacultyManagersQuery(
        FacultyId FacultyId)
    : IQuery<List<Staff>>;