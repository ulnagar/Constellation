namespace Constellation.Core.Models.Stocktake;

using Enums;
using Identifiers;

public sealed class Difference
{
    public Difference(
        StocktakeEventId eventId,
        StocktakeSightingId sightingId,
        DifferenceCategory category) 
    {
        Id = new();
    }

    private Difference() { }

    public DifferenceId Id { get; init; }
    public StocktakeEventId EventId { get; init; }
    public StocktakeSightingId SightingId { get; init; }
    public DifferenceCategory Category { get; init; }
    public bool Resolved { get; private set; }

    public void SetResolved() => Resolved = true;
}
