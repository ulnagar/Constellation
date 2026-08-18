namespace Constellation.Application.Domains.Students.Commands.UpdateStudentAbsenceConfiguration;

using Abstractions.Messaging;
using Core.Models.Absences.Enums;
using Core.Models.Students.Identifiers;
using System;

public sealed record UpdateStudentAbsenceConfigurationCommand(
    StudentId StudentId,
    AbsenceType Type,
    DateOnly EndDate)
    : ICommand;