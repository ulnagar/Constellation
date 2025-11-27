namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Errors;
using Extensions;
using System.Text.Json;

public sealed class TimetablePeriod
{
    public static Result<TimetablePeriod> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);
        
        if (!typeExists || type.GetString() != "timetablePeriod")
            return Result.Failure<TimetablePeriod>(SentralJsonErrors.IncorrectObject("TimetablePeriod", typeExists ? type.GetString() : string.Empty));
        
        TimetablePeriod timetablePeriod = new();
        timetablePeriod.Id = jsonEntry.ExtractString("id");
        
        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            timetablePeriod.Name = attributes.ExtractString("name");
            timetablePeriod.Order = attributes.ExtractString("order");
            timetablePeriod.IsActive = attributes.ExtractBool("isActive") ?? false;
        }
        return timetablePeriod;
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Order { get; private set; }
    public bool IsActive { get; private set; }
}