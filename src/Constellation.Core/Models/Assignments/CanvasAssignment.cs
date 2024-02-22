namespace Constellation.Core.Models.Assignments;

using DomainEvents;
using Constellation.Core.Errors;
using Identifiers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public class CanvasAssignment : AggregateRoot
{
    private readonly List<CanvasAssignmentSubmission> _submissions = new();

    // Required for EF Core
    public CanvasAssignment() { }

    private CanvasAssignment(
        CourseId courseId, 
        string name, 
        int canvasId, 
        DateTime dueDate, 
        DateTime? lockDate, 
        DateTime? unlockDate,
        bool delayForwarding,
        DateOnly forwardingDate,
        int allowedAttempts)
    {
        Id = new();
        CourseId = courseId;
        Name = name;
        CanvasId = canvasId;
        DueDate = dueDate;
        LockDate = lockDate;
        UnlockDate = unlockDate;
        DelayForwarding = delayForwarding;
        ForwardingDate = forwardingDate;
        AllowedAttempts = allowedAttempts;
    }

    public AssignmentId Id { get; }
    public CourseId CourseId { get; private set; }
    public string Name { get; private set; }
    public int CanvasId { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? LockDate { get; private set; }
    public DateTime? UnlockDate { get; private set; }
    public bool DelayForwarding { get; private set; }
    public DateOnly ForwardingDate { get; private set; }
    public int AllowedAttempts { get; private set; }
    public IReadOnlyCollection<CanvasAssignmentSubmission> Submissions => _submissions;

    public static CanvasAssignment Create(
        CourseId courseId,
        string name,
        int canvasId,
        DateTime dueDate,
        DateTime? lockDate,
        DateTime? unlockDate,
        bool delayForwarding,
        DateOnly? forwardingDate,
        int allowedAttempts)
    {
        CanvasAssignment entry = new(
            courseId,
            name,
            canvasId,
            dueDate,
            lockDate,
            unlockDate,
            delayForwarding,
            forwardingDate ?? DateOnly.MinValue,
            allowedAttempts);

        return entry;
    }

    public Result<CanvasAssignmentSubmission> AddSubmission(
        string studentId,
        string submittedBy)
    {
        bool existing = Submissions.Any(entry => entry.StudentId == studentId);

        int attempt = existing switch
        {
            true => Submissions.Where(entry => entry.StudentId == studentId).Max(entry => entry.Attempt) + 1,
            false => 1
        };

        CanvasAssignmentSubmission entry = CanvasAssignmentSubmission.Create(
            Id,
            studentId,
            submittedBy,
            DateTime.Now,
            attempt);

        RaiseDomainEvent(new AssignmentAttemptSubmittedDomainEvent(new DomainEventId(), Id, entry.Id));

        _submissions.Add(entry);

        return entry;
    }

    public void MarkSubmissionUploaded(
        AssignmentSubmissionId submissionId)
    {
        CanvasAssignmentSubmission submission = Submissions.FirstOrDefault(entry => entry.Id == submissionId);

        if (submission is not null)
            submission.MarkUploaded();
    }
}
