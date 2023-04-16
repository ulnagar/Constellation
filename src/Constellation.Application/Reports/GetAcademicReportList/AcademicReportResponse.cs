namespace Constellation.Application.Reports.GetAcademicReportList;

using Constellation.Core.Models.Identifiers;

public sealed record AcademicReportResponse(
    AcademicReportId Id,
    string PublishId,
    string Year,
    string ReportingPeriod);
