#nullable enable
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Shared;

namespace Constellation.Core.Models.Assets;

using Identifiers;
using System;

public sealed record Sighting
{
    // Required by EF Core
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

    public SightingId Id { get; private set; } = new();
    public AssetId AssetId { get; private set; }
    public string SightedBy { get; private set; } = string.Empty;
    public DateTime SightedAt { get; private set; }
    public string Note { get; private set; } = string.Empty;

    public static Result<Sighting> Create(
        AssetId assetId,
        string sightedBy,
        DateTime sightedAt,
        string note,
        IDateTimeProvider dateTime)
    {
        if (string.IsNullOrWhiteSpace(sightedBy))
            return Result.Failure<Sighting>(SightingErrors.NoWitness);

        if (sightedAt > dateTime.Now)
            return Result.Failure<Sighting>(SightingErrors.FutureSighting);

        Sighting sighting = new(
            assetId,
            sightedBy,
            sightedAt,
            note);

        return sighting;
    }
}