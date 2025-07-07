namespace Constellation.Core.Models.Stocktake.Errors;

using Identifiers;
using Shared;
using System;

public static class StocktakeEventErrors
{
    public static readonly Func<StocktakeEventId, Error> EventNotFound = id => new(
        "Stocktake.Event.NotFound",
        $"Could not find a Stocktake Event with the Id {id}");

    public static Error StartDateAfterEndDate => new(
        "Stocktake.Event.StartDateAfterEndDate",
        "Cannot create a stocktake event where the Start Date is later than the End Date");
}