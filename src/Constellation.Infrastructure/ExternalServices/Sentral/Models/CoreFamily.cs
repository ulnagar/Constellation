namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Shared;

public sealed class CoreFamily
{
    public static Result<CoreFamily> ConvertFromJson(dynamic jsonEntry)
    {
        if (jsonEntry["type"].ToString() != "coreFamily")
            return Result.Failure<CoreFamily>(SentralJsonErrors.IncorrectObject("CoreFamily", jsonEntry["type"].ToString()));

        CoreFamily family = new()
        {
            FamilyId = jsonEntry["id"].ToString(),
            AddressTitle = jsonEntry["attributes"]["addressTitle"].ToString(),
            AddressStreetNo = jsonEntry["attributes"]["addressStreetNo"].ToString(),
            AddressStreet = jsonEntry["attributes"]["addressStreet"].ToString(),
            AddressSuburb = jsonEntry["attributes"]["addressSuburb"].ToString(),
            AddressState = jsonEntry["attributes"]["addressState"].ToString(),
            AddressPostCode = jsonEntry["attributes"]["addressPostCode"].ToString(),
            PhoneNumber = jsonEntry["attributes"]["phone"].ToString(),
            EmailAddress = jsonEntry["attributes"]["emailAddress"].ToString()
        };

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