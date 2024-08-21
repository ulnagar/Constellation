namespace Constellation.Application.Assignments.Models;

using System;

public sealed record AssignmentFromCourseResponse(
    string Name,
    int CanvasAssignmentId,
    DateTime DueDate,
    DateTime? LockDate,
    DateTime? UnlockDate,
    int AllowedAttempts,
    bool ExistsInDatabase);