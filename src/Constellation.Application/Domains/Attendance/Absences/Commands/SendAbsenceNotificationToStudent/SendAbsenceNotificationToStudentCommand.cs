namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceNotificationToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using System;
using System.Collections.Generic;

public sealed record SendAbsenceNotificationToStudentCommand(
    Guid JobId,
    StudentId StudentId,
    List<AbsenceId> AbsenceIds)
    : ICommand;