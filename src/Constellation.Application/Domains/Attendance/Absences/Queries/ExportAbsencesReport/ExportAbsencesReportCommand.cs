namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportAbsencesReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record ExportAbsencesReportCommand(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<StudentId> StudentIds)
    : ICommand<FileDto>;