namespace Constellation.Application.WorkFlows.ExportOpenCaseReport;

using Core.Enums;
using Core.ValueObjects;
using System;

public sealed record CaseReportItem(
    string Id,
    Name Student,
    Grade Grade,
    string Description,
    DateOnly CreatedDate,
    DateOnly? CompletedDate,
    string AssignedTo,
    int OpenDays);