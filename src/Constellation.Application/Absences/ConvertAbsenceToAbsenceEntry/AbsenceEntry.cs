namespace Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record AbsenceEntry(
    AbsenceId Id,
    DateOnly Date,
    string PeriodName,
    string PeriodTimeframe,
    string OfferingName,
    string AbsenceTimeframe);
