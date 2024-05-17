namespace Constellation.Core.Models.Assets.Errors;

using Shared;

public static class AssetErrors
{
    public static readonly Error SerialNumberEmpty = new(
        "Assets.Asset.SerialNumberEmpty",
        "Serial Number cannot be empty");
}