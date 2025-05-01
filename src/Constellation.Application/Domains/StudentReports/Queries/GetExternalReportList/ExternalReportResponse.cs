namespace Constellation.Application.Domains.StudentReports.Queries.GetExternalReportList;

using Core.Models.Reports.Enums;
using Core.Models.Reports.Identifiers;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System;

public sealed record ExternalReportResponse(
    ExternalReportId Id,
    StudentId StudentId,
    Name StudentName,
    ReportType Type,
    DateOnly IssuedDate);