namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;
using Errors;
using Extensions;
using System.Text.Json;

public sealed class AbsenceReason
{
    public static Result<AbsenceReason> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);
        if (!typeExists || type.GetString() != "absenceReason")
            return Result.Failure<AbsenceReason>(SentralJsonErrors.IncorrectObject("AbsenceReason", typeExists ? type.GetString() : string.Empty));
        
        AbsenceReason reason = new();
        reason.Id = jsonEntry.ExtractString("id");
        
        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            reason.Description = attributes.ExtractString("description");
            reason.ShortHand = attributes.ExtractString("shorthand");
            reason.IsExplained = attributes.ExtractBool("isExplained") ?? false;
        }
        return reason;
    }


    public string Id { get; private set; }
    public string Description { get; private set; }
    public string ShortHand { get; private set; }
    public bool IsExplained { get; private set; }
}