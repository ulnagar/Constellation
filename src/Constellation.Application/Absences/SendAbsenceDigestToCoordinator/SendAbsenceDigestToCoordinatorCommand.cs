namespace Constellation.Application.Absences.SendAbsenceDigestToCoordinator;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System;

public sealed record SendAbsenceDigestToCoordinatorCommand(
    Guid JobId,
    StudentId StudentId)
    : ICommand;