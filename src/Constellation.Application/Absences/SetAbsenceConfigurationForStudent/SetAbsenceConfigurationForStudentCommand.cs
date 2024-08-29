namespace Constellation.Application.Absences.SetAbsenceConfigurationForStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Absences;
using Core.Models.Students.Identifiers;
using System;

public sealed record SetAbsenceConfigurationForStudentCommand(
    StudentId StudentId,
    string SchoolCode,
    int? GradeFilter,
    AbsenceType AbsenceType,
    DateOnly StartDate,
    DateOnly? EndDate)
    : ICommand;