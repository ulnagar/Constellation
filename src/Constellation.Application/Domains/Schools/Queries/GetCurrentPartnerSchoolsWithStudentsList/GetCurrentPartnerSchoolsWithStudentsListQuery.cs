namespace Constellation.Application.Domains.Schools.Queries.GetCurrentPartnerSchoolsWithStudentsList;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentPartnerSchoolsWithStudentsListQuery()
    : IQuery<List<SchoolSelectionListResponse>>;