namespace Constellation.Application.Absences.SendAbsenceDigestToStudent;

using Abstractions.Messaging;
using System;

public sealed record SendAbsenceDigestToStudentCommand(
    Guid JobId,
    string StudentId)
    : ICommand;