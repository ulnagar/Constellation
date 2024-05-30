namespace Constellation.Core.Models.Stocktake.Errors;

using Shared;
using System;

public static class StocktakeErrors
{
    public static Error SightingAlreadyCancelled => new(
        "Stocktake.Sighting.AlreadyCancelled",
        "The selected stocktake sighting record has already been cancelled");

    public static Error SightingInvalidSerialOrAsset => new(
        "Stocktake.Sighting.InvalidSerialOrAsset",
        "A valid Serial Number or Asset Number is required to register a sighting");

    public static readonly Func<Guid, Error> SightingNotFound = id => new(
            "Stocktake.Sighting.NotFound",
            $"Could not find a stocktake sighting record with the id {id}");
}
