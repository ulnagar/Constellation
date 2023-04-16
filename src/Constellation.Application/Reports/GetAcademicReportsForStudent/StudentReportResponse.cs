namespace Constellation.Application.Reports.GetAcademicReportsForStudent;

public sealed record StudentReportResponse(
    string StudentId,
    string PublishId,
    string Year,
    string ReportingPeriod);