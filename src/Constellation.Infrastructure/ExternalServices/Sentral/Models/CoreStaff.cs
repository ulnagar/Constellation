namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Constellation.Infrastructure.ExternalServices.Sentral.Errors;
using Core.Shared;
using Extensions;
using System.Text.Json;

public sealed class CoreStaff
{
    public static Result<CoreStaff> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "coreStaff")
            return Result.Failure<CoreStaff>(SentralJsonErrors.IncorrectObject("CoreStaff", typeExists ? type.GetString() : string.Empty));

        CoreStaff staff = new();
        staff.Id = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            staff.Title = attributes.ExtractString("preferredTitle");
            staff.FirstName = attributes.ExtractString("preferredFirstName");
            staff.LastName = attributes.ExtractString("preferredLastName");
            staff.Gender = attributes.ExtractString("gender");
            staff.EmailAddress = attributes.ExtractString("email");
            staff.ExternalId = attributes.ExtractString("externalId");
        }

        return staff;
    }

    public string Id { get; private set; }
    public string Title { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Gender { get; private set; }
    public string EmailAddress { get; private set; }
    public string ExternalId { get; private set; }

    public string Name => $"{Title} {FirstName} {LastName}";
}