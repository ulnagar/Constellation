namespace Constellation.Application.Assets.GetAssetByAssetNumber;

using Abstractions.Messaging;
using Core.Models.Assets;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAssetByAssetNumberQueryHandler
: IQueryHandler<GetAssetByAssetNumberQuery, AssetResponse>
{
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger _logger;

    public GetAssetByAssetNumberQueryHandler(
        IAssetRepository assetRepository,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _logger = logger.ForContext<GetAssetByAssetNumberQuery>();
    }

    public async Task<Result<AssetResponse>> Handle(GetAssetByAssetNumberQuery request, CancellationToken cancellationToken)
    {
        Asset asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            return Result.Failure<AssetResponse>(Error.NullValue);
        }

        Allocation lastAllocation = asset.CurrentAllocation;
        Location lastLocation = asset.CurrentLocation;
        Sighting lastSighting = asset.LastSighting;

        List<AssetResponse.AllocationDetails> allocations = new();

        foreach (Allocation allocation in asset.Allocations)
        {
            allocations.Add(new(
                allocation.Id,
                allocation.AllocationType,
                allocation.UserId,
                allocation.ResponsibleOfficer,
                allocation.AllocationDate,
                allocation.ReturnDate));
        }

        List<AssetResponse.LocationDetails> locations = new();

        foreach (Location location in asset.Locations)
        {
            locations.Add(new(
                location.Id,
                location.Category,
                location.Site,
                location.SchoolCode,
                location.Room,
                location.CurrentLocation,
                location.ArrivalDate,
                location.DepartureDate));
        }

        List<AssetResponse.SightingDetails> sightings = new();

        foreach (Sighting sighting in asset.Sightings)
        {
            sightings.Add(new(
                sighting.Id,
                sighting.SightedBy,
                sighting.SightedAt,
                sighting.Note));
        }

        List<AssetResponse.NoteDetails> notes = new();

        foreach (Note note in asset.Notes)
        {
            notes.Add(new(
                note.Id,
                note.Message,
                note.CreatedBy,
                note.CreatedAt));
        }


        return new AssetResponse(
            asset.Id,
            asset.AssetNumber,
            asset.SerialNumber,
            asset.SapEquipmentNumber,
            asset.Manufacturer,
            asset.ModelNumber,
            asset.ModelDescription,
            asset.Status,
            asset.Category,
            asset.PurchaseDate,
            asset.PurchaseDocument,
            asset.PurchaseCost,
            asset.WarrantyEndDate,
            lastAllocation?.Id,
            lastAllocation?.ResponsibleOfficer ?? string.Empty,
            lastAllocation?.AllocationDate ?? DateOnly.MinValue,
            lastLocation?.Id,
            lastLocation?.Category,
            lastLocation?.Site ?? string.Empty,
            lastLocation?.SchoolCode ?? string.Empty,
            lastLocation?.Room ?? string.Empty,
            lastLocation?.ArrivalDate ?? DateOnly.MinValue,
            lastSighting?.Id,
            lastSighting?.SightedAt ?? DateTime.MinValue,
            lastSighting?.SightedBy ?? string.Empty,
            lastSighting?.Note ?? string.Empty,
            allocations,
            locations, 
            sightings, 
            notes);
    }
}
