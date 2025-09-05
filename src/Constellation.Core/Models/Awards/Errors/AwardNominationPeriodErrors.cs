namespace Constellation.Core.Models.Awards.Errors;

using Constellation.Core.Models.Awards.Identifiers;
using Constellation.Core.Shared;
using System;

public static class AwardNominationPeriodErrors
{
    public static readonly Func<AwardNominationPeriodId, Error> NotFound = id => new(
        "Awards.NominationPeriod.NotFound",
        $"Could not find a nomination period with the id {id}");

    public static readonly Error PastDate = new(
        "Awards.NominationPeriod.PastDate",
        "The specified Lockout Date for the Nomination Period is in the past and invalid");
}