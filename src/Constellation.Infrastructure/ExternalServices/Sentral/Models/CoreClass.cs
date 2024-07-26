namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Enums;
using Core.Shared;
using Extensions;
using System.Text.Json;

public sealed class CoreClass
{
    public static Result<CoreClass> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "coreClass")
            return Result.Failure<CoreClass>(SentralJsonErrors.IncorrectObject("CoreClass", typeExists ? type.GetString() : string.Empty));

        CoreClass item = new();
        item.Id = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            item.Name = attributes.ExtractString("name");
            item.Description = attributes.ExtractString("description");
            item.Year = attributes.ExtractInt("year");
            item.ExternalId = attributes.ExtractString("externalId");
            item.IsActive = attributes.ExtractBool("isActive") ?? false;
        }

        string grade = attributes.ExtractString("schoolYear");
        if (!string.IsNullOrWhiteSpace(grade))
        {
            bool gradeSuccess = Enum.TryParse(grade, out Grade gradeItem);
            item.SchoolYear = gradeSuccess ? gradeItem : Grade.SpecialProgram;
        }
        
        bool relationshipsExists = jsonEntry.TryGetProperty("relationships", out JsonElement relationships);
        if (!relationshipsExists)
            return item;

        bool staffExists = relationships.TryGetProperty("teacher", out JsonElement staff);
        if (!staffExists)
            return item;
        
        item.TeacherId = staff.GetProperty("data").GetProperty("id").GetString();

        return item;
    }

    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Year { get; private set; }
    public Grade SchoolYear { get; private set; }
    public string ExternalId { get; private set; }
    public bool IsActive { get; private set; }
    public string TeacherId { get; private set; }
}