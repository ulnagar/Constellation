namespace Constellation.Application.Absences.SendAbsenceDigestToCoordinator;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record SendAbsenceDigestToCoordinatorCommand(
    Guid JobId,
    string StudentId)
    : ICommand;