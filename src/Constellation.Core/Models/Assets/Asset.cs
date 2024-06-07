#nullable enable
namespace Constellation.Core.Models.Assets;

using Abstractions.Clock;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Repositories;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ValueObjects;

public sealed class Asset : AggregateRoot, IAuditableEntity
{
    private readonly List<Allocation> _allocations = new();
    private readonly List<Location> _locations = new();
    private readonly List<Sighting> _sightings = new();
    private readonly List<Note> _notes = new();

    // Required by EF Core
    private Asset() { }

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
    public AssetNumber AssetNumber { get; private set; } = AssetNumber.Empty;
    public string SerialNumber { get; private set; } = string.Empty;
    public string? SapEquipmentNumber { get; private set; }
    public string Manufacturer { get; private set; } = string.Empty;
    public string ModelNumber { get; private set; } = string.Empty;
    public string ModelDescription { get; private set; } = string.Empty;
    public AssetStatus Status { get; private set; } = AssetStatus.Active;
    public AssetCategory Category { get; private set; } = AssetCategory.Student;
    public DateOnly PurchaseDate { get; private set; }
    public string PurchaseDocument { get; private set; } = string.Empty;
    public decimal PurchaseCost { get; private set; }
    public DateOnly WarrantyEndDate { get; private set; }
    public IReadOnlyList<Allocation> Allocations => _allocations.AsReadOnly();
    public IReadOnlyList<Location> Locations => _locations.AsReadOnly();
    public IReadOnlyList<Sighting> Sightings => _sightings.AsReadOnly();
    public IReadOnlyList<Note> Notes => _notes.AsReadOnly();

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

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static async Task<Result<Asset>> Create(
        AssetNumber assetNumber,
        string serialNumber,
        string? sapEquipmentNumber,
        string manufacturer,
        string modelNumber,
        string description,
        AssetCategory category,
        string purchaseDocument,
        decimal purchaseCost,
        DateOnly warrantyEndDate,
        IDateTimeProvider dateTime,
        IAssetRepository repository)
    {
        ArgumentNullException.ThrowIfNull(dateTime);

        if (string.IsNullOrWhiteSpace(serialNumber))
            return Result.Failure<Asset>(AssetErrors.SerialNumberEmpty);

        bool assetNumberTaken = await repository.IsAssetNumberTaken(assetNumber);

        if (assetNumberTaken)
            return Result.Failure<Asset>(AssetErrors.CreateAssetNumberTaken(assetNumber));

        Asset asset = new(
            assetNumber,
            serialNumber,
            description,
            category)
        {
            SapEquipmentNumber = string.IsNullOrWhiteSpace(sapEquipmentNumber) ? null : sapEquipmentNumber,
            Manufacturer = manufacturer,
            ModelNumber = modelNumber,
            PurchaseDocument = purchaseDocument,
            PurchaseCost = purchaseCost,
            WarrantyEndDate = warrantyEndDate,
            PurchaseDate = dateTime.Today,
            Status = AssetStatus.Active
        };

        asset.RaiseDomainEvent(new AssetRegisteredDomainEvent(new(), asset.Id, asset.AssetNumber));

        return asset;
    }

    public static async Task<Result<Asset>> Create(
        AssetNumber assetNumber,
        string serialNumber,
        string? sapEquipmentNumber,
        string manufacturer,
        string modelNumber,
        string description,
        AssetCategory category,
        DateOnly purchaseDate,
        string purchaseDocument,
        decimal purchaseCost,
        DateOnly warrantyEndDate,
        IAssetRepository repository)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return Result.Failure<Asset>(AssetErrors.SerialNumberEmpty);

        bool assetNumberTaken = await repository.IsAssetNumberTaken(assetNumber);

        if (assetNumberTaken)
            return Result.Failure<Asset>(AssetErrors.CreateAssetNumberTaken(assetNumber));

        Asset asset = new(
            assetNumber,
            serialNumber,
            description,
            category)
        {
            SapEquipmentNumber = string.IsNullOrWhiteSpace(sapEquipmentNumber) ? null : sapEquipmentNumber,
            Manufacturer = manufacturer,
            ModelNumber = modelNumber,
            PurchaseDocument = purchaseDocument,
            PurchaseCost = purchaseCost,
            WarrantyEndDate = warrantyEndDate,
            PurchaseDate = purchaseDate,
            Status = AssetStatus.Active
        };

        asset.RaiseDomainEvent(new AssetRegisteredDomainEvent(new(), asset.Id, asset.AssetNumber));

        return asset;
    }

    public static async Task<Result<Asset>> Create(
        AssetNumber assetNumber,
        string serialNumber,
        string manufacturer,
        string description,
        AssetCategory category,
        IDateTimeProvider dateTime,
        IAssetRepository repository)
    {
        ArgumentNullException.ThrowIfNull(dateTime);

        if (string.IsNullOrWhiteSpace(serialNumber))
            return Result.Failure<Asset>(AssetErrors.SerialNumberEmpty);

        bool assetNumberTaken = await repository.IsAssetNumberTaken(assetNumber);

        if (assetNumberTaken)
            return Result.Failure<Asset>(AssetErrors.CreateAssetNumberTaken(assetNumber));

        Asset asset = new(
            assetNumber,
            serialNumber,
            description,
            category)
        {
            Manufacturer = manufacturer,
            PurchaseDate = dateTime.Today,
            Status = AssetStatus.Active
        };

        asset.RaiseDomainEvent(new AssetRegisteredDomainEvent(new(), asset.Id, asset.AssetNumber));

        return asset;
    }

    public Result Update(
        string manufacturer,
        string modelNumber,
        string description,
        string sapEquipmentNumber,
        string purchaseDocument,
        decimal purchaseCost,
        DateOnly warrantyEndDate)
    {
        bool updatedProperty = false;

        List<string> messages = new();

        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            messages.Add($"{nameof(Manufacturer)} changed from {Manufacturer} to {manufacturer}");

            Manufacturer = manufacturer;

            updatedProperty = true;
        }

        if (!string.IsNullOrWhiteSpace(modelNumber))
        {
            messages.Add($"{nameof(ModelNumber)} changed from {ModelNumber} to {modelNumber}");

            ModelNumber = modelNumber;

            updatedProperty = true;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            messages.Add($"{nameof(ModelDescription)} changed from {ModelDescription} to {description}");

            ModelDescription = description;

            updatedProperty = true;
        }

        if (!string.IsNullOrWhiteSpace(sapEquipmentNumber))
        {
            messages.Add($"{nameof(SapEquipmentNumber)} changed from {SapEquipmentNumber} to {sapEquipmentNumber}");

            SapEquipmentNumber = sapEquipmentNumber;

            updatedProperty = true;
        }

        if (!string.IsNullOrWhiteSpace(purchaseDocument))
        {
            messages.Add($"{nameof(purchaseDocument)} changed from {PurchaseDocument} to {purchaseDocument}");
            
            PurchaseDocument = purchaseDocument;

            updatedProperty = true;
        }

        if (purchaseCost > 0m)
        {
            messages.Add($"{nameof(PurchaseCost)} changed from {PurchaseCost} to {purchaseCost}");

            PurchaseCost = purchaseCost;

            updatedProperty = true;
        }

        if (warrantyEndDate > DateOnly.MinValue)
        {
            messages.Add($"{nameof(WarrantyEndDate)} changed from {WarrantyEndDate} to {warrantyEndDate}");

            WarrantyEndDate = warrantyEndDate;

            updatedProperty = true;
        }

        if (!updatedProperty)
        {
            return Result.Failure(AssetErrors.UpdateNoChangeDetected);
        }

        Result<Note> note = Note.Create(Id, string.Join(Environment.NewLine, messages));

        if (note.IsFailure)
            return Result.Failure(note.Error);

        _notes.Add(note.Value);

        return Result.Success();
    }

    public Result UpdateCategory(AssetCategory category)
    {
        if (Category.Equals(category))
            return Result.Failure(AssetErrors.UpdateCategoryNoChange);

        Result<Note> note = Note.Create(Id, $"{nameof(Category)} changed from {Category} to {category}");

        if (note.IsFailure)
            return Result.Failure(note.Error);

        Category = category;

        return Result.Success();
    }

    public Result UpdateStatus(AssetStatus status)
    {
        if (Status.Equals(status))
            return Result.Failure(AssetErrors.UpdateStatusNoChange);

        if (Status.Equals(AssetStatus.Disposed) && !status.Equals(AssetStatus.Disposed))
            return Result.Failure(AssetErrors.UpdateStatusReactivateDisposedAsset);

        Result<Note> note = Note.Create(Id, $"{nameof(Status)} changed from {Status} to {status}");

        if (note.IsFailure)
            return Result.Failure(note.Error);

        Status = status;

        return Result.Success();
    }

    public void AddAllocation(Allocation allocation) => _allocations.Add(allocation);
    public void AddLocation(Location location) => _locations.Add(location);
    public void AddSighting(Sighting sighting) => _sightings.Add(sighting);
    public void AddNote(Note note) => _notes.Add(note);

    public void Delete() => IsDeleted = true;
}
