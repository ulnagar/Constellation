namespace Constellation.Core.Models.Assets.Errors;

using Shared;

public static class AssetNumberErrors
{
    public static readonly Error Empty = new(
        "Assets.AssetNumber.Empty",
        "AssetNumber must not be empty");

    public static readonly Error TooLong = new(
        "Assets.AssetNumber.TooLong",
        "AssetNumber must be no larger than 8 numbers");

    public static readonly Error UnknownCharacters = new(
        "Assets.AssetNumber.UnknownCharacters",
        "AssetNumber must be numbers only");
}