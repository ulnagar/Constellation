namespace Constellation.Application.Absences.ExportAbsencesReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using System.Collections.Generic;

public sealed record ExportAbsencesReportCommand(
    List<int> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<string> StudentIds)
    : ICommand<FileDto>;