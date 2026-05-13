namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentSubmissionsForDownload;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using DTOs;

public sealed record GetAssessmentSubmissionsForDownloadQuery(
    AssessmentId AssessmentId)
    : IQuery<FileDto>;