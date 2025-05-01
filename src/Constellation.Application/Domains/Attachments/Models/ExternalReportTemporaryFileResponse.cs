namespace Constellation.Application.Domains.Attachments.Models;

using Core.Models.Reports.Enums;
using Core.Models.Reports.Identifiers;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System;

public sealed record ExternalReportTemporaryFileResponse(
    ExternalReportId ReportId,
    string FileName,
    StudentId StudentId,
    Name StudentName,
    ReportType ReportType,
    DateOnly IssuedDate);
