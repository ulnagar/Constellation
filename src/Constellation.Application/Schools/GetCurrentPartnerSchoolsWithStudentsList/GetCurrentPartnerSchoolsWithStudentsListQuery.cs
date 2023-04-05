namespace Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Schools.Models;
using System.Collections.Generic;

public sealed record GetCurrentPartnerSchoolsWithStudentsListQuery()
    : IQuery<List<SchoolSelectionListResponse>>;