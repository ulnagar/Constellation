namespace Constellation.Application.Domains.StudentReports.Queries.GetCombinedReportListForStudent;

using Core.Models.Reports.Enums;
using Core.Models.Reports.Identifiers;
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
