﻿namespace Constellation.Application.Assignments.CreateAssignment;

using Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
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
