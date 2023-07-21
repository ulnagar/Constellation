namespace Constellation.Application.Absences.SendAbsenceDigestToParent;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record SendAbsenceDigestToParentCommand(
    Guid JobId,
    string StudentId)
    : ICommand;