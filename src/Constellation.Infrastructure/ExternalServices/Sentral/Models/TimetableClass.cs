namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Errors;
using Extensions;
using System.Text.Json;

public sealed class TimetableClass
{
    public static Result<TimetableClass> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "timetableClass")
            return Result.Failure<TimetableClass>(SentralJsonErrors.IncorrectObject("TimetableClass", typeExists ? type.GetString() : string.Empty));

        TimetableClass timetableClass = new();
        timetableClass.Id = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            timetableClass.Name = attributes.ExtractString("name");
        }

        return timetableClass;
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
}