namespace Constellation.Application.Reports.GetStudentReportsForSchool;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;

public sealed record SchoolStudentReportResponse(
        string StudentId,
        Name Name,
        Grade Grade,
        AcademicReportId ReportId,
        string PublishId,
        string Year,
        string ReportingPeriod);