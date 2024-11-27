namespace Constellation.Application.Attachments.GetTemporaryFileById;

using Abstractions.Messaging;
using Core.Models.Reports.Identifiers;
using Models;

public sealed record GetTemporaryFileByIdQuery(
    ExternalReportId ReportId)
    : IQuery<ExternalReportTemporaryFileResponse>;