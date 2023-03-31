namespace Constellation.Application.Assignments.GetAssignmentById;

using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record AssignmentResponse(
    AssignmentId AssignmentId,
    int CourseId,
    string CourseName,
    string AssignmentName,
    DateOnly DueDate,
    DateOnly? UnlockDate,
    DateOnly? LockDate,
    int AllowedAttempts,
    List<AssignmentResponse.Submission> Submissions)
{
    public sealed record Submission(
        AssignmentSubmissionId SubmissionId,
        string StudentName,
        DateOnly SubmittedDate,
        int AttemptNumber);
}