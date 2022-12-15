namespace Constellation.Core.Models;

public class StudentAward
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public virtual Student? Student { get; set; }
    public DateTime AwardedOn { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
