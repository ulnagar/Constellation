namespace Constellation.Application.Absences.SendAbsenceNotificationToParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;
using System;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;

public sealed record SendAbsenceNotificationToParentCommand(
    Guid JobId,
    string StudentId,
    List<AbsenceId> AbsenceIds)
    : ICommand;