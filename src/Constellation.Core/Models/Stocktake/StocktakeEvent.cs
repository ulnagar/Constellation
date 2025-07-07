namespace Constellation.Core.Models.Stocktake;

using Assets.ValueObjects;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Stocktake.Enums;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class StocktakeEvent : AggregateRoot
{
    private readonly List<StocktakeSighting> _sightings = [];
    private readonly List<Difference> _differences = [];

    public StocktakeEvent(
        string name,
        DateTime startDate,
        DateTime endDate,
        bool acceptLateResponses)
    {
        Id = new();
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        AcceptLateResponses = acceptLateResponses;
    }

    public StocktakeEventId Id { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; } 
    public bool AcceptLateResponses { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyCollection<StocktakeSighting> Sightings => _sightings.AsReadOnly();
    public IReadOnlyCollection<Difference> Differences => _differences.AsReadOnly();

    public Result Update(
        string name,
        DateTime startDate,
        DateTime endDate,
        bool acceptLateResponses)
    {
        if (startDate > endDate)
            return Result.Failure(StocktakeEventErrors.StartDateAfterEndDate);

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        AcceptLateResponses = acceptLateResponses;

        return Result.Success();
    }

    public Result AddSighting(
        string serialNumber,
        AssetNumber assetNumber,
        string description,
        LocationCategory locationCategory,
        string locationName,
        string locationCode,
        UserType userType,
        string userName,
        string userCode,
        string comment,
        string sightedBy,
        DateTime sightedAt,
        DifferenceCategory changes)
    {
        Result<StocktakeSighting> sighting = StocktakeSighting.Create(
            Id,
            serialNumber,
            assetNumber,
            description,
            locationCategory,
            locationName,
            locationCode,
            userType,
            userName,
            userCode,
            comment,
            sightedBy,
            sightedAt);

        if (sighting.IsFailure)
            return sighting;

        _sightings.Add(sighting.Value);

        if (!changes.Equals(DifferenceCategory.None))
        {
            Difference difference = new(
                Id,
                sighting.Value.Id,
                changes);

            _differences.Add(difference);
        }

        return Result.Success();
    }

    public Result CancelSighting(
        StocktakeSightingId sightingId,
        string comment,
        string cancelledBy)
    {
        StocktakeSighting sighting = _sightings.FirstOrDefault(entry => entry.Id == sightingId);

        if (sighting is null)
            return Result.Failure(StocktakeSightingErrors.SightingNotFound(sightingId));

        return sighting.Cancel(comment, cancelledBy);
    }

    public Result ResolveDifference(
        DifferenceId differenceId)
    {
        Difference difference = _differences.FirstOrDefault(entry => entry.Id == differenceId);

        if (difference is null)
            return Result.Failure(StocktakeDifferenceErrors.DifferenceNotFound(differenceId));

        difference.SetResolved();

        return Result.Success();
    }
}