namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesWithFilterForReport;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
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
