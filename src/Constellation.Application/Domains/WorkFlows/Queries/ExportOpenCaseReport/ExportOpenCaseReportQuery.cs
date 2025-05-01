namespace Constellation.Application.Domains.WorkFlows.Queries.ExportOpenCaseReport;

using Abstractions.Messaging;
using DTOs;

public sealed record ExportOpenCaseReportQuery
    : IQuery<FileDto>;