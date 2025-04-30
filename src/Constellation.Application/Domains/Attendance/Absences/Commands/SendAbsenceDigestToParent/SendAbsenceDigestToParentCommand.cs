namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceDigestToParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using System;

public sealed record SendAbsenceDigestToParentCommand(
    Guid JobId,
    StudentId StudentId)
    : ICommand;