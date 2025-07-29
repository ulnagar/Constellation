namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceNotificationToParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using System;
using System.Collections.Generic;

public sealed record SendAbsenceNotificationToParentCommand(
    Guid JobId,
    StudentId StudentId,
    List<AbsenceId> AbsenceIds)
    : ICommand;