namespace Constellation.Application.Absences.SendAbsenceDigestToStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System;

public sealed record SendAbsenceDigestToStudentCommand(
    Guid JobId,
    StudentId StudentId)
    : ICommand;