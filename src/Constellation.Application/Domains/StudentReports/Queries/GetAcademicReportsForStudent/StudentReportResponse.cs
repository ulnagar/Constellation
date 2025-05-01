namespace Constellation.Application.Domains.StudentReports.Queries.GetAcademicReportsForStudent;

using Core.Models.Students.Identifiers;

public sealed record StudentReportResponse(
    StudentId StudentId,
    string PublishId,
    string Year,
    string ReportingPeriod);