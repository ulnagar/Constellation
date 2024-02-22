namespace Constellation.Application.Absences.ExportUnexplainedPartialAbsencesReport;

using Core.Enums;
using Core.Models.Identifiers;
using System;

public sealed record UnexplainedPartialAbsenceResponse(
    AbsenceId AbsenceId,
    string StudentId,
    string FirstName,
    string LastName,
    Grade Grade,
    string SchoolName,
    DateOnly AbsenceDate,
    string OfferingName,
    int AbsenceLength,
    string AbsenceTimeframe,
    string AbsenceStatus);
