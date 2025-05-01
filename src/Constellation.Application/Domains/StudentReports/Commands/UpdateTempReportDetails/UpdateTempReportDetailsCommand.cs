namespace Constellation.Application.Domains.StudentReports.Commands.UpdateTempReportDetails;

using Abstractions.Messaging;
using Core.Models.Reports.Enums;
using Core.Models.Reports.Identifiers;
using Core.Models.Students.Identifiers;
using System;

public sealed record UpdateTempReportDetailsCommand(
    ExternalReportId ReportId,
    StudentId StudentId,
    ReportType ReportType,
    DateOnly IssuedDate)
    : ICommand;
