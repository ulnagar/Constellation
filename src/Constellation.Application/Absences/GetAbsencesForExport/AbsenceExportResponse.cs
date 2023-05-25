namespace Constellation.Application.Absences.GetAbsencesForExport;

using Constellation.Core.Enums;
using Constellation.Core.Models.Absences;
using Constellation.Core.ValueObjects;
using System;

public sealed record AbsenceExportResponse(
    Name Student,
    Grade StudentGrade,
    string SchoolName,
    bool Explained,
    AbsenceType Type,
    DateOnly Date,
    string Period,
    int Length,
    string Timeframe,
    string Class,
    int NotificationCount,
    int ResponseCount);