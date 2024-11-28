namespace Constellation.Application.Reports.GetAllAcademicReports;

using Core.Models.Reports.Identifiers;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record AcademicReportSummary(
    AcademicReportId ReportId,
    StudentId StudentId,
    Name StudentName,
    string Year,
    string ReportPeriod);