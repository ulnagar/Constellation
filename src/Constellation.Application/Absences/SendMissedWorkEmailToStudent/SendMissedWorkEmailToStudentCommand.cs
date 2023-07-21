namespace Constellation.Application.Absences.SendMissedWorkEmailToStudent;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record SendMissedWorkEmailToStudentCommand(
    Guid JobId,
    string StudentId)
    : ICommand;