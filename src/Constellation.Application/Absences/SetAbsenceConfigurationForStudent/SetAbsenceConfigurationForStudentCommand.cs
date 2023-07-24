namespace Constellation.Application.Absences.SetAbsenceConfigurationForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences;
using System;

public sealed record SetAbsenceConfigurationForStudentCommand(
    string StudentId,
    string SchoolCode,
    int? GradeFilter,
    AbsenceType AbsenceType,
    DateOnly StartDate,
    DateOnly? EndDate)
    : ICommand;