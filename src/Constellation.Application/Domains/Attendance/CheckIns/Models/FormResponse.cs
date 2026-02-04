namespace Constellation.Application.Domains.Attendance.CheckIns.Models;

using System.Text.Json.Serialization;

public sealed class FormResponse
{
    public string? EmailAddress { get; set; }
    [JsonPropertyName("SubmittedAt")]
    public string? StringDate { get; set; }
    public string? Sentiment { get; set; }
    public string? Subject { get; set; }
    public GroupOption Group { get; set; }

    public DateTime? Submitted => string.IsNullOrWhiteSpace(StringDate) ? null : DateTime.Parse(StringDate).ToLocalTime();
}