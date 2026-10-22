namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Errors;
using Extensions;
using System.Text.Json;

public sealed class StudentUnavailability
{
    public static Result<StudentUnavailability> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "studentUnavailability")
            return Result.Failure<StudentUnavailability>(SentralJsonErrors.IncorrectObject("StudentUnavailability", typeExists ? type.GetString() : string.Empty));

        StudentUnavailability unavailability = new();
        unavailability.Id = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            unavailability.Date = attributes.ExtractDateOnly("date") ?? DateOnly.MinValue;
            unavailability.StartTime = attributes.ExtractTimeOnly("startTime") ?? TimeOnly.MinValue;
            unavailability.EndTime = attributes.ExtractTimeOnly("endTime") ?? TimeOnly.MinValue;
        }

        return unavailability;
    }

    public string Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}