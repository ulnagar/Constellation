namespace Constellation.Core.Models;

public class CanvasAssignment
{
    public Guid Id { get; set; }
    public virtual Course? Course { get; set; }
    public int? CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CanvasId { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? LockDate { get; set; }
    public DateTime? UnlockDate { get; set; }
    public int AllowedAttempts { get; set; }
}
