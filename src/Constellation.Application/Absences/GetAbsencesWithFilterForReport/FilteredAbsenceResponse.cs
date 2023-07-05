namespace Constellation.Application.Absences.GetAbsencesWithFilterForReport;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;

public sealed record FilteredAbsenceResponse(
    Name Student,
    string School,
    Grade Grade,
    string OfferingName,
    AbsenceId AbsenceId,
    bool IsExplained,
    DateOnly AbsenceDate,
    AbsenceType Type,
    string AbsenceTimeframe,
    string PeriodName,
    int NotificationCount,
    int ResponseCount);
