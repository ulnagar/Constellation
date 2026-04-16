namespace Constellation.Core.Models.Assessments.Archive;

using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Primitives;
using Identifiers;
using System;
using System.Collections.Generic;

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

    public AssignmentId Id { get; } = new();
    public CourseId CourseId { get; private set; } = CourseId.Empty;
    public string Name { get; private set; } = string.Empty;
    public int CanvasId { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? LockDate { get; private set; }
    public DateTime? UnlockDate { get; private set; }
    public bool DelayForwarding { get; private set; }
    public DateOnly ForwardingDate { get; private set; }
    public int AllowedAttempts { get; private set; }
    public IReadOnlyCollection<CanvasAssignmentSubmission> Submissions => _submissions;
}
