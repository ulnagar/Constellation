﻿namespace Constellation.Core.Models.Stocktake.Errors;

using Identifiers;
using Shared;
using System;

public static class StocktakeSightingErrors
{
    public static Error SightingAlreadyCancelled => new(
        "Stocktake.Sighting.AlreadyCancelled",
        "The selected stocktake sighting record has already been cancelled");

    public static Error SightingInvalidSerialOrAsset => new(
        "Stocktake.Sighting.InvalidSerialOrAsset",
        "A valid Serial Number or Asset Number is required to register a sighting");

    public static readonly Func<StocktakeSightingId, Error> SightingNotFound = id => new(
        "Stocktake.Sighting.NotFound",
        $"Could not find a stocktake sighting record with the id {id}");

    public static Error AssetNotAtSite = new(
        "Stocktake.Sighting.AssetNotAtSite",
        "This asset is registered at a different location. Please check the Asset Number or Serial Number again. If this error persists, please contact the Technology Support Team on 1300 610 733.");
}