namespace Constellation.Core.Models.Edval.Errors;

using Constellation.Core.Models.Edval.Identifiers;
using Shared;
using System;

public static class DifferenceErrors
{
    public static Func<DifferenceId, Error> NotFound = (id) => new(
        "Edval.Difference",
        $"A Difference with the Id {id} could not be found");
}
