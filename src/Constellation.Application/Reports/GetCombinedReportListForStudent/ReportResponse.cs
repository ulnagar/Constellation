namespace Constellation.Application.Reports.GetCombinedReportListForStudent;

using Constellation.Core.Models.Reports.Identifiers;
using Core.Models.Reports.Enums;
using System;

public abstract record ReportResponse(
    string Year);

public sealed record AcademicReportResponse(
    AcademicReportId Id,
    string PublishId,
    string Year,
    string ReportingPeriod)
    : ReportResponse(Year);

public sealed record ExternalReportResponse(
    ExternalReportId Id,
    ReportType Type,
    DateOnly IssuedDate)
    : ReportResponse(IssuedDate.Year.ToString());
