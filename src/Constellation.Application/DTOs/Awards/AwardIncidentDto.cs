namespace Constellation.Application.DTOs.Awards;

using System;

public sealed class AwardIncidentDto
{
    public DateOnly DateIssued { get; set; }
    public string IncidentId { get; set; }
    public string TeacherName { get; set; }
    public string IssueReason { get; set; }
}