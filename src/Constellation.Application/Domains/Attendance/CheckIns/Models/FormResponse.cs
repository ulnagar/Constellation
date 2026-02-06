namespace Constellation.Application.Domains.Attendance.CheckIns.Models;

using System.Globalization;
using System.Text.Json.Serialization;

public sealed class FormResponse
{
    public string? EmailAddress { get; set; }
    [JsonPropertyName("SubmittedAt")]
    public string? StringDate { get; set; }
    public string? Sentiment { get; set; }
    public string? Subject { get; set; }
    public GroupOption Group { get; set; }

    public DateTime? Submitted => GetSubmittedDateTime();

    private DateTime? GetSubmittedDateTime()
    {
        if (string.IsNullOrWhiteSpace(StringDate))
            return null;

        string[] parts = StringDate.Split(' ');

        string date = parts[0];

        string[] dateParts = date.Split('/');
        int day = Convert.ToInt32(dateParts[0], CultureInfo.InvariantCulture);
        int month = Convert.ToInt32(dateParts[1], CultureInfo.InvariantCulture);
        int year = Convert.ToInt32(dateParts[2], CultureInfo.InvariantCulture);

        string[] timeParts = parts[1].Split(':');
        int hours = Convert.ToInt32(timeParts[0], CultureInfo.InvariantCulture);
        int minutes = Convert.ToInt32(timeParts[1], CultureInfo.InvariantCulture);
        int seconds = Convert.ToInt32(timeParts[2], CultureInfo.InvariantCulture);

        hours = parts[2] == "AM" ? hours : hours + 12;

        DateTime returnDate =
            (DateTime.UtcNow.Day == day)
                ? new DateTime(year, month, day, hours, minutes, seconds, DateTimeKind.Utc).ToLocalTime()
                : new DateTime(year, day, month, hours, minutes, seconds, DateTimeKind.Utc).ToLocalTime();

        return returnDate;
    }
}