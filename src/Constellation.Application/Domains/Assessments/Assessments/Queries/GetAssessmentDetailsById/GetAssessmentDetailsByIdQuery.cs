namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDetailsById;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Models;

public sealed record GetAssessmentDetailsByIdQuery(
    AssessmentId Id)
    : IQuery<AssessmentDetailsResponse>;