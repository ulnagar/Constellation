#nullable enable
namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAssetByAssetNumber;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Identifiers;
using Constellation.Core.Models.Assets.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record AssetResponse(
    AssetId AssetId,
    AssetNumber AssetNumber,
    string SerialNumber,
    string SapEquipmentNumber,
    string Manufacturer,
    string ModelNumber,
    string ModelDescription,
    AssetStatus Status,
    AssetCategory Category,
    DateOnly PurchaseDate,
    string PurchaseDocument,
    decimal PurchaseCost,
    DateOnly WarrantyEndDate,
    AllocationId? AllocationId,
    string AllocatedTo,
    DateOnly AllocatedOn,
    LocationId? LocationId,
    LocationCategory? LocationCategory,
    string Site,
    string SchoolCode,
    string Room,
    DateOnly ArrivalDate,
    SightingId? SightingId,
    DateTime SightedAt,
    string SightedBy,
    string SightingNotes,
    IReadOnlyList<AssetResponse.AllocationDetails> Allocations,
    IReadOnlyList<AssetResponse.LocationDetails> Locations,
    IReadOnlyList<AssetResponse.SightingDetails> Sightings,
    IReadOnlyList<AssetResponse.NoteDetails> Notes)
{
    public sealed record AllocationDetails(
        AllocationId AllocationId,
        AllocationType AllocationType,
        string UserId,
        string UserName,
        DateOnly AllocationDate,
        DateOnly ReturnDate);

    public sealed record LocationDetails(
        LocationId LocationId,
        LocationCategory Category,
        string Site,
        string SchoolCode,
        string Room,
        bool CurrentLocation,
        DateOnly ArrivalDate,
        DateOnly DepartureDate);

    public sealed record SightingDetails(
        SightingId SightingId,
        string SightedBy,
        DateTime SightedAt,
        string Note);

    public sealed record NoteDetails(
        NoteId NoteId,
        string Message,
        string CreatedBy,
        DateTime CreatedAt);
}