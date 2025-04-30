namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceDigestToCoordinator;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using System;

public sealed record SendAbsenceDigestToCoordinatorCommand(
    Guid JobId,
    StudentId StudentId)
    : ICommand;