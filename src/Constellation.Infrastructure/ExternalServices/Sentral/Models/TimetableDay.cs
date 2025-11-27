namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Errors;
using Extensions;
using System.Text.Json;

public sealed class TimetableDay
{
    public static Result<TimetableDay> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);
        
        if (!typeExists || type.GetString() != "timetableDay")
            return Result.Failure<TimetableDay>(SentralJsonErrors.IncorrectObject("TimetableDay", typeExists ? type.GetString() : string.Empty));
        
        TimetableDay timetableDay = new();
        timetableDay.Id = jsonEntry.ExtractString("id");
        
        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            timetableDay.Name = attributes.ExtractString("name");
            timetableDay.IsActive = attributes.ExtractBool("isActive") ?? false;
        }
        return timetableDay;
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
}