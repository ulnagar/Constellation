namespace Constellation.Core.Models.Assets.Errors;

using Constellation.Core.Shared;

public static class LocationErrors
{
    public static readonly Error SiteEmpty = new(
        "Assets.Location.SiteEmpty",
        "A site name is required to create a Location for an Asset");

    public static readonly Error UnknownCategory = new(
        "Assets.Location.UnknownCategory",
        "Unrecognised LocationCategory provided");
}