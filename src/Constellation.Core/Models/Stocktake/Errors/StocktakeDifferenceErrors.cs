namespace Constellation.Core.Models.Stocktake.Errors;

using Identifiers;
using Shared;
using System;

public static class StocktakeDifferenceErrors
{
    public static readonly Func<DifferenceId, Error> DifferenceNotFound = id => new(
        "Stocktake.Difference.NotFound",
        $"Could not find a Stocktake Difference record with the id {id}");
}