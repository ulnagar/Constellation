namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Constellation.Infrastructure.ExternalServices.Sentral.Errors;
using Core.Shared;
using Extensions;
using System.Text.Json;

public sealed class CoreDate
{
    public static Result<CoreDate> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "date")
            return Result.Failure<CoreDate>(SentralJsonErrors.IncorrectObject("Date", typeExists ? type.GetString() : string.Empty));

        CoreDate date = new();
        date.Date = jsonEntry.ExtractDateOnly("id") ?? DateOnly.MinValue;

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            date.Term = attributes.ExtractString("term");
            date.Week = attributes.ExtractString("week");
            date.Code = attributes.ExtractString("code");
        }

        return date;
    }

    public DateOnly Date { get; private set; }
    public string Term { get; private set; }
    public string Week { get; private set; }
    public string Code { get; private set; }
}