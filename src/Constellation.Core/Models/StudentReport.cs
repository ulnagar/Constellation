namespace Constellation.Core.Models;

public class StudentReport
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public virtual Student? Student { get; set; }
    public string PublishId { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string ReportingPeriod { get; set; } = string.Empty;
}
