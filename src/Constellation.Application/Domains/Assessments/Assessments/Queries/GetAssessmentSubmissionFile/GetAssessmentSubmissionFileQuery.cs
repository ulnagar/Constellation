namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentSubmissionFile;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using DTOs;

public sealed record GetAssessmentSubmissionFileQuery(
    AssessmentId AssessmentId,
    SubmissionId SubmissionId)
    : IQuery<FileDto>;
