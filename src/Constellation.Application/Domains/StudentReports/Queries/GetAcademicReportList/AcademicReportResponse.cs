namespace Constellation.Application.Domains.StudentReports.Queries.GetAcademicReportList;

using Core.Models.Reports.Identifiers;

public sealed record AcademicReportResponse(
    AcademicReportId Id,
    string PublishId,
    string Year,
    string ReportingPeriod);
