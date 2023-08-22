namespace Constellation.Application.Absences.GetAbsenceForStudentResponse;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.ValueObjects;
using System;

public sealed record AbsenceForStudentResponse(
    AbsenceId AbsenceId,
    Name Student,
    string StudentId,
    OfferingId OfferingId,
    string OfferingName,
    DateOnly AbsenceDate,
    AbsenceType AbsenceType,
    string PeriodName,
    string PeriodTimeframe,
    string AbsenceTimeframe,
    int AbsenceLength);
