namespace Constellation.Core.Models.Assets;

using Abstractions.Clock;
using Abstractions.Services;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class Asset : AggregateRoot, IAuditableEntity
{
    private readonly List<Allocation> _allocations = new();
    private readonly List<Location> _locations = new();
    private readonly List<Sighting> _sightings = new();

    private Asset(
        AssetNumber assetNumber,
        string serialNumber,
        string description,
        AssetCategory category)
    {
        AssetNumber = assetNumber;
        SerialNumber = serialNumber;
        ModelDescription = description;
        Category = category;

        Status = AssetStatus.Active;
    }

    public AssetId Id { get; private set; } = new();
    public AssetNumber AssetNumber { get; private set; }
    public string SerialNumber { get; private set; }
    public string SapEquipmentNumber { get; private set; }
    public string ModelNumber { get; private set; }
    public string ModelDescription { get; private set; }
    public AssetStatus Status { get; private set; }
    public AssetCategory Category { get; private set; }
    public DateOnly PurchaseDate { get; private set; }
    public string PurchaseDocument { get; private set; }
    public decimal PurchaseCost { get; private set; }
    public DateOnly WarrantyEndDate { get; private set; }
    public DateOnly LastSightedAt { get; private set; }
    public string LastSightedBy { get; private set; }
    public IReadOnlyList<Allocation> Allocations => _allocations.AsReadOnly();
    public IReadOnlyList<Location> Locations => _locations.AsReadOnly();
    public IReadOnlyList<Sighting> Sightings => _sightings.AsReadOnly();

    public Allocation? CurrentAllocation => 
        _allocations
            .Where(allocation => allocation.ReturnDate == DateOnly.MinValue)
            .MaxBy(allocation => allocation.AllocationDate);

    public Location? CurrentLocation =>
        _locations
            .Where(location => location.DepartureDate == DateOnly.MinValue)
            .MaxBy(location => location.ArrivalDate);

    public Sighting? LastSighting =>
        _sightings
            .MaxBy(sighting => sighting.SightedAt);

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Result<Asset> Create(
        AssetNumber assetNumber,
        string serialNumber,
        string description,
        AssetCategory category,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return Result.Failure<Asset>(AssetErrors.Asset.SerialNumberEmpty);

        Asset asset = new(
            assetNumber,
            serialNumber,
            description,
            category)
        {
            PurchaseDate = dateTime.Today, 
            LastSightedAt = dateTime.Today, 
            LastSightedBy = currentUserService.UserName
        };

        asset.RaiseDomainEvent(new AssetRegisteredDomainEvent(new(), asset.Id, asset.AssetNumber));

        return asset;
    }

    public void AddAllocation(Allocation allocation) => _allocations.Add(allocation);
    public void AddLocation(Location location) => _locations.Add(location);
    public void AddSighting(Sighting sighting) => _sightings.Add(sighting);
}
