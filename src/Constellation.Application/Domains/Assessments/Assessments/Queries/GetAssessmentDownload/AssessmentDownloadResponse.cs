namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownload;

using Constellation.Core.Models.Assessments.Identifiers;
using System;

public sealed record AssessmentDownloadResponse(
    AssessmentId AssessmentId,
    AssessmentDownloadId AssessmentDownloadId,
    string Name,
    DateOnly AvailableFrom,
    DateOnly AvailableTo,
    bool IsRestricted,
    bool IsActive);