namespace Constellation.Application.Reports.GetStudentReportsForSchool;

using Constellation.Core.Enums;
using Constellation.Core.Models.Reports.Identifiers;

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