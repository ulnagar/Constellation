namespace Constellation.Application.Domains.StudentReports.Queries.GetStudentReportsForSchool;

using Core.Enums;
using Core.Models.Reports.Identifiers;

public sealed record SchoolStudentReportResponse(
        string StudentId,
        string FirstName,
        string LastName,
        string DisplayName,
        Grade Grade,
        AcademicReportId ReportId,
        string PublishId,
        string Year,
        string ReportingPeriod);