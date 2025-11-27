namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Errors;
using Extensions;
using System.Text.Json;

public sealed class TimetablePeriodInDay
{
    public static Result<TimetablePeriodInDay> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "timetablePeriodInDay")
            return Result.Failure<TimetablePeriodInDay>(SentralJsonErrors.IncorrectObject("TimetablePeriodInDay", typeExists ? type.GetString() : string.Empty));

        TimetablePeriodInDay timetablePeriodInDay = new();
        timetablePeriodInDay.Id = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            timetablePeriodInDay.Name = attributes.ExtractString("name");
            timetablePeriodInDay.Order = attributes.ExtractInt("order");
            timetablePeriodInDay.StartTime = attributes.ExtractTimeOnly("startTime") ?? TimeOnly.MinValue;
            timetablePeriodInDay.EndTime = attributes.ExtractTimeOnly("endTime") ?? TimeOnly.MinValue;
            timetablePeriodInDay.IsActive = attributes.ExtractBool("isActive") ?? false;
        }

        bool relationshipsExists = jsonEntry.TryGetProperty("relationships", out JsonElement relationships);
        if (relationshipsExists)
        {
            bool dayExists = relationships.TryGetProperty("day", out JsonElement timetableDay);
            if (dayExists)
            {
                timetablePeriodInDay.TimetableDayId = timetableDay.GetProperty("data").ExtractString("id");
            }

            bool periodExists = relationships.TryGetProperty("period", out JsonElement timetablePeriod);
            if (periodExists)
            {
                timetablePeriodInDay.TimetablePeriodId = timetablePeriod.GetProperty("data").ExtractString("id");
            }
        }

        return timetablePeriodInDay;
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public int Order { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsActive { get; private set; }
    public string TimetableDayId { get; private set; }
    public string TimetablePeriodId { get; private set; }
}