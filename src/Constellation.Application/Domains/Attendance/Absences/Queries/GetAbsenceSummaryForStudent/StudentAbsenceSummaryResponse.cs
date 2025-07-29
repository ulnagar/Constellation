namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceSummaryForStudent;

using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using System;

public sealed record StudentAbsenceSummaryResponse(
    AbsenceId AbsenceId,
    bool IsExplained,
    AbsenceType AbsenceType,
    DateOnly AbsenceDate,
    string Timeframe,
    string PeriodName,
    string OfferingName,
    bool PendingVerification);