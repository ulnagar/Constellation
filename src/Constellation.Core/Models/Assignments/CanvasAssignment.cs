namespace Constellation.Core.Models.Assignments;

using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public class CanvasAssignment : AggregateRoot
{
    private readonly List<CanvasAssignmentSubmission> _submissions = new();

    private CanvasAssignment(
        AssignmentId id, 
        CourseId courseId, 
        string name, 
        int canvasId, 
        DateTime dueDate, 
        DateTime? lockDate, 
        DateTime? unlockDate, 
        int allowedAttempts)
    {
        Id = id;
        CourseId = courseId;
        Name = name;
        CanvasId = canvasId;
        DueDate = dueDate;
        LockDate = lockDate;
        UnlockDate = unlockDate;
        AllowedAttempts = allowedAttempts;
    }

    public AssignmentId Id { get; private set; }
    public CourseId CourseId { get; private set; }
    public string Name { get; private set; }
    public int CanvasId { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? LockDate { get; private set; }
    public DateTime? UnlockDate { get; private set; }
    public int AllowedAttempts { get; private set; }
    public IReadOnlyCollection<CanvasAssignmentSubmission> Submissions => _submissions;

    public static CanvasAssignment Create(
        AssignmentId id,
        CourseId courseId,
        string name,
        int canvasId,
        DateTime dueDate,
        DateTime? lockDate,
        DateTime? unlockDate,
        int allowedAttempts)
    {
        CanvasAssignment entry = new(
            id,
            courseId,
            name,
            canvasId,
            dueDate,
            lockDate,
            unlockDate,
            allowedAttempts);

        return entry;
    }

    public Result<CanvasAssignmentSubmission> AddSubmission(
        AssignmentSubmissionId submissionId,
        string studentId)
    {
        var existing = Submissions.Any(entry => entry.StudentId == studentId);

        var attempt = existing switch
        {
            true => Submissions.Where(entry => entry.StudentId == studentId).Max(entry => entry.Attempt) + 1,
            false => 1
        };

        var entry = CanvasAssignmentSubmission.Create(
            submissionId,
            Id,
            studentId,
            DateTime.Now,
            attempt);

        RaiseDomainEvent(new AssignmentAttemptSubmittedDomainEvent(new DomainEventId(), Id, submissionId));

        _submissions.Add(entry);

        return entry;
    }

    public Result ReuploadSubmissionToCanvas(
        AssignmentSubmissionId submissionId)
    {
        var submission = Submissions.FirstOrDefault(entry => entry.Id == submissionId);

        if (submission is null)
            return Result.Failure(DomainErrors.Assignments.Submission.NotFound(submissionId));

        RaiseDomainEvent(new AssignmentAttemptSubmittedDomainEvent(new DomainEventId(), Id, submissionId));

        return Result.Success();
    }
}
