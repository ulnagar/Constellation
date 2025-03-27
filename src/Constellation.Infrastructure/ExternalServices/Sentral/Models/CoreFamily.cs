namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Constellation.Infrastructure.ExternalServices.Sentral.Errors;
using Core.Shared;
using Extensions;
using System.Text.Json;

public sealed class CoreFamily
{
    public static Result<CoreFamily> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "coreFamily")
            return Result.Failure<CoreFamily>(SentralJsonErrors.IncorrectObject("CoreFamily", typeExists ? type.GetString() : string.Empty));

        CoreFamily family = new();
        family.FamilyId = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            family.AddressTitle = attributes.ExtractString("addressTitle");
            family.AddressStreetNo = attributes.ExtractString("addressStreetNo");
            family.AddressStreet = attributes.ExtractString("addressStreet");
            family.AddressSuburb = attributes.ExtractString("addressSuburb");
            family.AddressState = attributes.ExtractString("addressState");
            family.AddressPostCode = attributes.ExtractString("addressPostCode");
            family.PhoneNumber = attributes.ExtractString("phone");
            family.EmailAddress = attributes.ExtractString("emailAddress");
        }

        return family;
    }


    public string FamilyId { get; private set; }
    public string AddressTitle { get; private set; }
    public string AddressStreetNo { get; private set; }
    public string AddressStreet { get; private set; }
    public string AddressSuburb { get; private set; }
    public string AddressState { get; private set; }
    public string AddressPostCode { get; private set; }
    public string PhoneNumber { get; private set; }
    public string EmailAddress { get; private set;}
}