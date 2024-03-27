namespace Constellation.Application.Absences.ExportAbsencesReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record ExportAbsencesReportCommand(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<string> StudentIds)
    : ICommand<FileDto>;