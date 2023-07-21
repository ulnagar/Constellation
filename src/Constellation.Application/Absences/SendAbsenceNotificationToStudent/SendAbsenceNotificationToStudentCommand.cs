namespace Constellation.Application.Absences.SendAbsenceNotificationToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record SendAbsenceNotificationToStudentCommand(
    Guid JobId,
    string StudentId,
    List<AbsenceId> AbsenceIds)
    : ICommand;