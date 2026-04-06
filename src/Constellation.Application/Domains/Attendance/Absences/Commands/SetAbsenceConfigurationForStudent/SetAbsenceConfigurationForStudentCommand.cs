namespace Constellation.Application.Domains.Attendance.Absences.Commands.SetAbsenceConfigurationForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Core.Models.Identifiers;
using System;

public sealed record SetAbsenceConfigurationForStudentCommand(
    StudentId StudentId,
    SchoolCode SchoolCode,
    int? GradeFilter,
    AbsenceType AbsenceType,
    DateOnly StartDate,
    DateOnly? EndDate)
    : ICommand;