namespace Constellation.Application.WorkFlows.ExportOpenCaseReport;

using Abstractions.Messaging;
using DTOs;

public sealed record ExportOpenCaseReportQuery
    : IQuery<FileDto>;