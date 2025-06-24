namespace Constellation.Core.Models.Stocktake;

using Assets.ValueObjects;
using Enums;
using Errors;
using Identifiers;
using Shared;
using System;

public sealed class StocktakeSighting
{
    /// <summary>
    /// For EF Core Only
    /// </summary>
    private StocktakeSighting() { }

    private StocktakeSighting(
        StocktakeEventId stocktakeEventId,
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
        DateTime sightedAt)
    {
        Id = new();
        StocktakeEventId = stocktakeEventId;
        SerialNumber = serialNumber;
        AssetNumber = assetNumber;
        Description = description;
        LocationCategory = locationCategory;
        LocationName = locationName;
        LocationCode = locationCode;
        UserType = userType;
        UserName = userName;
        UserCode = userCode;
        Comment = comment;
        SightedBy = sightedBy;
        SightedAt = sightedAt;
    }

    public StocktakeSightingId Id { get; private set; }
    public StocktakeEventId StocktakeEventId { get; private set; }
    public string SerialNumber { get; private set; }
    public AssetNumber AssetNumber { get; private set; }
    public string Description { get; private set; }
    public LocationCategory LocationCategory { get; private set; }
    public string LocationName { get; private set; }

    // If the LocationCategory is LocationCategories.PublicSchool, populate with school code for lookup
    public string LocationCode { get; private set; }
    public UserType UserType { get; private set; }
    public string UserName { get; private set; }

    // If the UserType is UserTypes.Student, populate with the student id for lookup
    // If the UserType is UserTypes.Staff, populate with the staff id for lookup
    // If the UserType is UserTypes.School, populate with the school code for lookup
    public string UserCode { get; private set; }
    public string Comment { get; private set; }
    public string SightedBy { get; private set; }
    public DateTime SightedAt { get; private set; }

    // If the sighting was incorrect for some reason, allow the user to cancel it if they provide a reason
    public bool IsCancelled { get; private set; }
    public string CancellationComment { get; private set; }
    public string CancelledBy { get; private set; }
    public DateTime CancelledAt { get; private set; }

    internal static Result<StocktakeSighting> Create(
        StocktakeEventId stocktakeEventId,
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
        DateTime sightedAt)
    {
        if (string.IsNullOrWhiteSpace(serialNumber) && string.IsNullOrWhiteSpace(assetNumber))
            return Result.Failure<StocktakeSighting>(StocktakeSightingErrors.SightingInvalidSerialOrAsset);

        StocktakeSighting sighting = new(
            stocktakeEventId,
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

        return sighting;
    }

    internal Result Cancel(
        string comment,
        string cancelledBy)
    {
        if (IsCancelled)
            return Result.Failure(StocktakeSightingErrors.SightingAlreadyCancelled);

        IsCancelled = true;
        CancellationComment = comment;
        CancelledBy = cancelledBy;
        CancelledAt = DateTime.Now;

        return Result.Success();
    }
}