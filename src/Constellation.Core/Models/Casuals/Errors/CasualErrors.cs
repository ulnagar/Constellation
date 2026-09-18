namespace Constellation.Core.Models.Casuals.Errors;

using Constellation.Core.Models.Identifiers;
using Shared;
using System;

public static class CasualErrors
{
    public static readonly Func<CasualId, Error> NotFound = id => new Error(
        "Casuals.Casual.NotFound",
        $"A Casual with the Id {id.Value} could not be found");
}