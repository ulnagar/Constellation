namespace Constellation.Application.DTOs;

using Core.ValueObjects;

public class RollMarkReportDto
{
    // Imported from API
    public DateTime Date { get; set; }
    public string Period { get; set; }
    public string ClassName { get; set; }
    public string Teacher { get; set; }
    public string Year { get; set; }
    public string Room { get; set; }
    public bool Submitted { get; set; }
}

public class RollMarkingEmailDto 
{
    public string RollInformation { get; set; }
    public List<EmailRecipient> Teachers { get; set; } = new();
    public List<EmailRecipient> HeadTeachers { get; set; } = new();
    public string Faculty { get; set; }
    public List<string> Notes { get; set; } = new();
    public string TeacherName { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
}