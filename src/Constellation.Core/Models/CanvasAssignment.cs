using System;

namespace Constellation.Core.Models
{
    public class CanvasAssignment
    {
        public Guid Id { get; set; }
        public Course Course { get; set; }
        public int? CourseId { get; set; }
        public string Name { get; set; }
        public int CanvasId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? LockDate { get; set; }
        public DateTime? UnlockDate { get; set; }
        public int AllowedAttempts { get; set; }
    }
}
