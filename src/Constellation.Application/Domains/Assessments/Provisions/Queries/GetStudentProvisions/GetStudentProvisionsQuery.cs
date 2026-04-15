namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetStudentProvisions;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetStudentProvisionsQuery()
    : IQuery<List<StudentProvisionResponse>>;