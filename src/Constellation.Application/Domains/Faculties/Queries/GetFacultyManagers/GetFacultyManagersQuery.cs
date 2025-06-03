namespace Constellation.Application.Domains.Faculties.Queries.GetFacultyManagers;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;
using Core.Models.StaffMembers;
using System.Collections.Generic;

public sealed record GetFacultyManagersQuery(
        FacultyId FacultyId)
    : IQuery<List<StaffMember>>;