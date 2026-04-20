namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentById;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using GetCurrentAssessments;
using Models;

public sealed record GetAssessmentByIdQuery(
    AssessmentId Id)
    : IQuery<AssessmentResponse>;
