namespace Constellation.Core.Models.Offerings.Errors;

using Shared;

public static class OfferingNameErrors 
{
    public static readonly Error ValueEmpty = new(
        "Offerings.OfferingName.Validation.ValueEmpty",
        "An empty value cannot be converted to an OfferingName");
}
