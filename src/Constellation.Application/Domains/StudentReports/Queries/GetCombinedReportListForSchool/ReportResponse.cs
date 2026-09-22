namespace Constellation.Application.Domains.StudentReports.Queries.GetCombinedReportListForSchool;

using Core.Enums;
using Core.Models.Reports.Enums;
using Core.Models.Reports.Identifiers;
using Core.ValueObjects;
using System;

public abstract record SchoolReportResponse(
    string StudentId,
    Name Student,
    Grade Grade,
    string Year);

public sealed record SchoolAcademicReportResponse(
    string StudentId,
    Name Student,
    Grade Grade,
    AcademicReportId Id,
    string PublishId,
    string Year,
    string ReportingPeriod)
    : SchoolReportResponse(
        StudentId,
        Student,
        Grade,
        Year);

public sealed record SchoolExternalReportResponse(
    string StudentId,
    Name Student,
    Grade Grade,
    ExternalReportId Id,
    ReportType Type,
    DateOnly IssuedDate)
    : SchoolReportResponse(
        StudentId,
        Student,
        Grade,
        IssuedDate.Year.ToString());
