namespace Constellation.Core.Models.Assets;

using Identifiers;
using System;

public sealed record Sighting
{
    private Sighting() { }

    private Sighting(
        AssetId assetId,
        string sightedBy,
        DateTime sightedAt,
        string note)
    {
        AssetId = assetId;
        SightedBy = sightedBy;
        SightedAt = sightedAt;
        Note = note;
    }

    public SightingId Id { get; set; } = new();
    public AssetId AssetId { get; private set; }
    public string SightedBy { get; private set; } = string.Empty;
    public DateTime SightedAt { get; private set; }
    public string Note { get; private set; } = string.Empty;

    public static Sighting Create(
        AssetId assetId,
        string sightedBy,
        DateTime sightedAt,
        string note)
    {
        Sighting sighting = new(
            assetId,
            sightedBy,
            sightedAt,
            note);

        return sighting;
    }
}
