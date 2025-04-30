namespace Constellation.Application.Domains.Attendance.Absences.Commands.SetAbsenceConfigurationForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Students.Identifiers;
using System;

public sealed record SetAbsenceConfigurationForStudentCommand(
    StudentId StudentId,
    string SchoolCode,
    int? GradeFilter,
    AbsenceType AbsenceType,
    DateOnly StartDate,
    DateOnly? EndDate)
    : ICommand;