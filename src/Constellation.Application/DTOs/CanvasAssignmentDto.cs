using Constellation.Core.Models.Assignments;
using System;

namespace Constellation.Application.DTOs
{
    public class CanvasAssignmentDto
    {
        public Guid Id { get; set; }
        public int? CourseId { get; set; }
        public string Name { get; set; }
        public int CanvasId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? LockDate { get; set; }
        public DateTime? UnlockDate { get; set; }
        public int AllowedAttempts { get; set; }

        public static CanvasAssignmentDto ConvertFromAssignment(CanvasAssignment assignment)
        {
            var viewModel = new CanvasAssignmentDto
            {
                Id = assignment.Id.Value,
                CourseId = assignment.CourseId,
                Name = assignment.Name,
                DueDate = assignment.DueDate,
                LockDate = assignment.LockDate,
                UnlockDate = assignment.UnlockDate,
                AllowedAttempts = assignment.AllowedAttempts
            };

            return viewModel;
        }
    }
}
