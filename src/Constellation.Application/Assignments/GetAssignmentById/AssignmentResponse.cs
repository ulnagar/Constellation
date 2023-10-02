namespace Constellation.Application.Assignments.GetAssignmentById;

using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;

public sealed record AssignmentResponse(
    AssignmentId AssignmentId,
    CourseId CourseId,
    string CourseName,
    string AssignmentName,
    DateOnly DueDate,
    DateOnly? UnlockDate,
    DateOnly? LockDate,
    bool DelayForwarding,
    DateOnly ForwardingDate,
    int AllowedAttempts,
    List<AssignmentResponse.Submission> Submissions)
{
    public sealed record Submission(
        AssignmentSubmissionId SubmissionId,
        string StudentName,
        DateOnly SubmittedDate,
        int AttemptNumber);
}