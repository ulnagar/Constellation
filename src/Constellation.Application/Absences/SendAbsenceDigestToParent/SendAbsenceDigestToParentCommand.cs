namespace Constellation.Application.Absences.SendAbsenceDigestToParent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System;

public sealed record SendAbsenceDigestToParentCommand(
    Guid JobId,
    StudentId StudentId)
    : ICommand;