namespace Constellation.Core.Models.Assignments;

using Constellation.Core.Models.Assignments.Identifiers;
using System;

public class CanvasAssignmentSubmission
{
    private CanvasAssignmentSubmission(
        AssignmentId assignmentId,
        string studentId,
        string submittedBy,
        DateTime submittedOn,
        int attempt)
    {
        Id = new();
        AssignmentId = assignmentId;
        StudentId = studentId;
        SubmittedBy = submittedBy;
        SubmittedOn = submittedOn;
        Attempt = attempt;
    }

    public AssignmentSubmissionId Id { get; private set; }
    public AssignmentId AssignmentId { get; private set; }
    public string StudentId { get; private set; }
    public string SubmittedBy { get; private set; }
    public DateTime SubmittedOn { get; private set; }
    public int Attempt { get; private set; }
    public bool Uploaded { get; private set; }

    public static CanvasAssignmentSubmission Create(
        AssignmentId assignmentId,
        string studentId,
        string submittedBy,
        DateTime submittedOn,
        int attempt)
    {
        CanvasAssignmentSubmission submission = new(
            assignmentId,
            studentId,
            submittedBy,
            submittedOn,
            attempt);

        return submission;
    }

    internal void MarkUploaded() => Uploaded = true;
}
