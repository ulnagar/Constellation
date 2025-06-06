﻿namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForSchool;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record SchoolAbsenceDetailsResponse(
    string StudentName,
    string ClassName,
    AbsenceId AbsenceId,
    DateTime AbsenceDate,
    string PeriodName,
    string AbsenceTimeframe);
