namespace Constellation.Application.DTOs;

using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Assessments.Archive;
using System;

public class CanvasAssignmentDto
{
    public Guid Id { get; set; }
    public CourseId CourseId { get; set; }
    public string CourseName { get; set; }
    public string AssignmentName { get; set; }
    public int CanvasId { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public DateTimeOffset? LockDate { get; set; }
    public DateTimeOffset? UnlockDate { get; set; }
    public int AllowedAttempts { get; set; }

    public static CanvasAssignmentDto ConvertFromAssignment(CanvasAssignment assignment)
    {
        var viewModel = new CanvasAssignmentDto
        {
            Id = assignment.Id.Value,
            CourseId = assignment.CourseId,
            AssignmentName = assignment.Name,
            DueDate = assignment.DueDate,
            LockDate = assignment.LockDate,
            UnlockDate = assignment.UnlockDate,
            AllowedAttempts = assignment.AllowedAttempts
        };

        return viewModel;
    }
}