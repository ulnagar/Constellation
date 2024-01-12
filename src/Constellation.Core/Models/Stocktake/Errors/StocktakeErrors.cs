namespace Constellation.Core.Models.Stocktake.Errors;

using Shared;
using System;

public static class StocktakeErrors
{
    public static class Sighting
    {
        public static readonly Func<Guid, Error> NotFound = id => new(
            "Stocktake.Sighting.NotFound",
            $"Could not find a stocktake sighting record with the id {id}");

        public static Error AlreadyCancelled => new(
            "Stocktake.Sighting.AlreadyCancelled",
            "The selected stocktake sighting record has already been cancelled");

        public static Error InvalidSerialOrAsset => new(
            "Stocktake.Sighting.InvalidSerialOrAsset",
            "A valid Serial Number or Asset Number is required to register a sighting");
    }
}
