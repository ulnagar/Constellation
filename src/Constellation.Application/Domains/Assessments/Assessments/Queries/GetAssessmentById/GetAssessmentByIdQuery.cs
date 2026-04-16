namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentById;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using GetCurrentAssessments;

public sealed record GetAssessmentByIdQuery(
    AssessmentId Id)
    : IQuery<AssessmentResponse>;
