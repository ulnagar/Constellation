namespace Constellation.Application.Absences.SendAbsenceNotificationToParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System;

public sealed record SendAbsenceNotificationToParentCommand(
    Guid JobId,
    string StudentId,
    List<AbsenceId> AbsenceIds)
    : ICommand;