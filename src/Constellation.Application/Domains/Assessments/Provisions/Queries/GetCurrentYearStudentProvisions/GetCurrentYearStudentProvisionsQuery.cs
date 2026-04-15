namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetCurrentYearStudentProvisions;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentYearStudentProvisionsQuery()
    : IQuery<List<StudentProvisionResponse>>;