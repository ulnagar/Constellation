using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;
using System;

namespace Constellation.Application.Features.Subject.Assignments.Models
{
    public class AssignmentForList : IMapFrom<CanvasAssignment>
    {
        public Guid Id { get; set; }
        public string CourseName { get; set; }
        public string CourseGrade { get; set; }
        public string CourseDisplayName => $"{CourseGrade} {CourseName}";
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? LockDate { get; set; }
        public DateTime? UnlockDate { get; set; }
        public int AllowedAttempts { get; set; }
    }
}
