using Constellation.Core.Shared;

namespace Constellation.Core.Models.Assets.Errors;

public static class SightingErrors
{
    public static readonly Error NoWitness = new(
        "Assets.Sighting.NoWitness",
        "A Staff Member must be selected as the witness for a sighting");

    public static readonly Error FutureSighting = new(
        "Assets.Sighting.FutureSighting",
        "A Sighting must have occurred in the past");
}