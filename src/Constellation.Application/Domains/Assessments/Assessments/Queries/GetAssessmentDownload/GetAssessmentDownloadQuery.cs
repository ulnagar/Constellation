namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownload;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;

public sealed record GetAssessmentDownloadQuery(
    AssessmentId AssessmentId,
    AssessmentDownloadId AssessmentDownloadId)
    : IQuery<AssessmentDownloadResponse>;