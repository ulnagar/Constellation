namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownloadFile;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Attachments.DTOs;

public sealed record GetAssessmentDownloadFileQuery(
    AssessmentId AssessmentId,
    AssessmentDownloadId DownloadId)
    : IQuery<AttachmentResponse>;
