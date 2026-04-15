namespace Constellation.Application.Domains.Assessments.Provisions.Queries.GetAssessmentProvisions;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAssessmentProvisionsQuery()
    : IQuery<List<AssessmentProvisionResponse>>;
