namespace Constellation.Application.Absences.SendAbsenceNotificationToStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using System;
using System.Collections.Generic;

public sealed record SendAbsenceNotificationToStudentCommand(
    Guid JobId,
    StudentId StudentId,
    List<AbsenceId> AbsenceIds)
    : ICommand;