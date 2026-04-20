namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCurrentAssessments;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentAssessmentsQuery()
    : IQuery<List<AssessmentResponse>>;