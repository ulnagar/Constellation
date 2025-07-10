namespace Constellation.Core.Models.Covers.Errors;

using Constellation.Core.Models.Covers.Identifiers;
using Shared;
using System;

public static class CoverErrors
{
    public static readonly Error CouldNotDetermineCoverType = new (
        "Covers.CoverType",
        "Could not determine Cover Type");

    public static readonly Func<CoverId, Error> NotFound = id => new Error(
        "Covers.CoverNotFound",
        $"A Cover with the Id {id.Value} could not be found");
}
