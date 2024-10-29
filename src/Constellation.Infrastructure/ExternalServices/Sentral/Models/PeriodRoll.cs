namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Extensions;
using System.Text.Json;

public sealed class PeriodRoll
{
    public static Result<PeriodRoll> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "periodRoll")
            return Result.Failure<PeriodRoll>(SentralJsonErrors.IncorrectObject("PeriodRoll", typeExists ? type.GetString() : string.Empty));

        PeriodRoll roll = new();
        roll.Id = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            roll.Date = attributes.ExtractDateOnly("date") ?? DateOnly.MinValue;
            roll.Period = attributes.ExtractInt("period");
            roll.PeriodName = attributes.ExtractString("periodName");
            roll.ClassTime = attributes.ExtractInt("classTime");
            roll.IsSubmitted = attributes.ExtractBool("isSubmitted") ?? false;
            roll.StartTime = attributes.ExtractTimeOnly("startTime") ?? TimeOnly.MinValue;
            roll.EndTime = attributes.ExtractTimeOnly("endTime") ?? TimeOnly.MinValue;
            roll.RollMarkingUrl = attributes.ExtractString("rollMarkingUrl");
        }

        return roll;
    }

    public string Id { get; private set; }
    public DateOnly Date { get; private set; }
    public int Period { get; private set; }
    public string PeriodName { get; private set; }
    public int ClassTime { get; private set; }
    public bool IsSubmitted { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public string RollMarkingUrl { get; private set; }
}