namespace Constellation.Application.Domains.StudentReports.Queries.GetCombinedReportListForSchool;

using Core.Enums;
using Core.Models.Reports.Enums;
using Core.Models.Reports.Identifiers;
using System;

public abstract record SchoolReportResponse(
    string StudentId,
    string FirstName,
    string LastName,
    string DisplayName,
    Grade Grade,
    string Year);

public sealed record SchoolAcademicReportResponse(
    string StudentId,
    string FirstName,
    string LastName,
    string DisplayName,
    Grade Grade,
    AcademicReportId Id,
    string PublishId,
    string Year,
    string ReportingPeriod)
    : SchoolReportResponse(
        StudentId,
        FirstName,
        LastName,
        DisplayName,
        Grade,
        Year);

public sealed record SchoolExternalReportResponse(
    string StudentId,
    string FirstName,
    string LastName,
    string DisplayName,
    Grade Grade,
    ExternalReportId Id,
    ReportType Type,
    DateOnly IssuedDate)
    : SchoolReportResponse(
        StudentId,
        FirstName,
        LastName,
        DisplayName,
        Grade,
        IssuedDate.Year.ToString());
