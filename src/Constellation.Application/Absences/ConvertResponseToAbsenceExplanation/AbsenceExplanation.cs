﻿namespace Constellation.Application.Absences.ConvertResponseToAbsenceExplanation;

using System;

public sealed record AbsenceExplanation(
    DateOnly Date,
    string PeriodName,
    string PeriodTimeframe,
    string OfferingName,
    string AbsenceTimeframe,
    string Explanation);