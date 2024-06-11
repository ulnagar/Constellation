using Constellation.Core.Models.Assets.Identifiers;

namespace Constellation.Core.Models.Assets.Errors;

using Constellation.Core.Models.Assets.ValueObjects;
using Shared;
using System;

public static class AssetErrors
{
    public static readonly Func<AssetNumber, Error> NotFoundByAssetNumber = id => new(
        "Assets.Asset.NotFound",
        $"An Asset with the Asset Number {id} could not be found");

    public static readonly Func<AssetId, Error> NotFoundById = id => new(
        "Assets.Asset.NotFound",
        $"An Asset with the Id {id} could not be found");

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

    public static readonly Func<AssetNumber, Error> CreateAssetNumberTaken = id => new(
        "Assets.Asset.Create.AssetNumberTaken",
        $"The Asset Number {id} has already been registered");

    public static readonly Error CannotUpdateDisposedItem = new(
        "Assets.Asset.UpdateDisposedItem",
        "Cannot update an item that has been marked as no longer active");
}