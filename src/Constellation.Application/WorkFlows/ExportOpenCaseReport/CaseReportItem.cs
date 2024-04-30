namespace Constellation.Application.WorkFlows.ExportOpenCaseReport;

using System;

public sealed record CaseReportItem(
    string Id,
    string Description,
    DateOnly CreatedDate,
    DateOnly? CompletedDate,
    string AssignedTo,
    int OpenDays);