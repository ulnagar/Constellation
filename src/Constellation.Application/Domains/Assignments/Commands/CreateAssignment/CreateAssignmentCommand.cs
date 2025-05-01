namespace Constellation.Application.Domains.Assignments.Commands.CreateAssignment;

using Abstractions.Messaging;
using Core.Models.Subjects.Identifiers;
using System;

public sealed record CreateAssignmentCommand(
        CourseId CourseId,
        string Name,
        int CanvasAssignmentId,
        DateTime DueDate,
        DateTime? LockDate,
        DateTime? UnlockDate,
        int AllowedAttempts,
        bool DelayForwarding,
        DateOnly ForwardDate)
    : ICommand;
