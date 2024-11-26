namespace Constellation.Application.Attachments.GetTemporaryFiles;

using Core.Models.Attachments.Identifiers;
using Core.Models.Reports.Enums;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record ExternalReportTemporaryFileResponse(
    AttachmentId AttachmentId,
    string FileName,
    StudentId StudentId,
    Name StudentName,
    ReportType? ReportType);
