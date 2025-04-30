namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceDigestToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using System;

public sealed record SendAbsenceDigestToStudentCommand(
    Guid JobId,
    StudentId StudentId)
    : ICommand;