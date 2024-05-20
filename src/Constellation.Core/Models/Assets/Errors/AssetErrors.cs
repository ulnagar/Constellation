namespace Constellation.Core.Models.Assets.Errors;

using Shared;

public static class AssetErrors
{
    public static readonly Error SerialNumberEmpty = new(
        "Assets.Asset.SerialNumberEmpty",
        "Serial Number cannot be empty");

    public static readonly Error UpdateNoChangeDetected = new(
        "Assets.Asset.Update.NoChangeDetected",
        "No additional data was provided to update the Asset details");

    public static readonly Error UpdateCategoryNoChange = new(
        "Assets.Asset.UpdateCategory.NoChange",
        "Cannot update the category to the current value");

    public static readonly Error UpdateStatusNoChange = new(
        "Assets.Asset.UpdateStatus.NoChange",
        "Cannot update the status to the current value");

    public static readonly Error UpdateStatusReactivateDisposedAsset = new(
        "Assets.Asset.UpdateStatus.ReactivateDisposedAsset",
        "Cannot reactivate an asset that has already been disposed");
}