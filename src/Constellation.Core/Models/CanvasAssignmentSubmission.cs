﻿namespace Constellation.Core.Models;

public class CanvasAssignmentSubmission
{
    public Guid Id { get; set; }
    public virtual CanvasAssignment? Assignment { get; set; }
    public Guid? AssignmentId { get; set; }
    public virtual Student? Student { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public int Attempt { get; set; }
}
