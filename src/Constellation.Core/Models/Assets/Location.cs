#nullable enable
namespace Constellation.Core.Models.Assets;

using Enums;
using Identifiers;
using Primitives;
using System;

public sealed record Location : IAuditableEntity
{
    // Required by EF Core
    private Location() { }

    private Location(
        AssetId assetId,
        LocationCategory category,
        string site,
        string schoolCode,
        string room,
        bool current,
        DateOnly arrivalDate)
    {
        AssetId = assetId;
        Category = category;
        Site = site;
        SchoolCode = schoolCode;
        Room = room;
        CurrentLocation = current;

        ArrivalDate = arrivalDate;
    }

    public LocationId Id { get; private set; } = new();
    public AssetId AssetId { get; private set; }
    public LocationCategory Category { get; private set; } = LocationCategory.CoordinatingOffice;
    public string Site { get; private set; } = string.Empty;
    public string SchoolCode { get; private set; } = string.Empty;
    public string Room { get; private set; } = string.Empty;
    public bool CurrentLocation { get; private set; }

    public DateOnly ArrivalDate { get; private set; }
    public DateOnly DepartureDate { get; private set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Location CreateOfficeLocationRecord(
        AssetId assetId,
        string room,
        bool current,
        DateOnly arrivalDate)
    {
        Location location = new(
            assetId,
            LocationCategory.CoordinatingOffice,
            "Aurora College",
            "8912",
            room,
            current,
            arrivalDate);

        return location;
    }

    public static Location CreatePublicSchoolLocationRecord(
        AssetId assetId,
        string site,
        string schoolCode,
        bool current,
        DateOnly arrivalDate)
    {
        Location location = new(
            assetId,
            LocationCategory.PublicSchool,
            site,
            schoolCode,
            string.Empty,
            current,
            arrivalDate);

        return location;
    }

    public static Location CreateCorporateOfficeLocationRecord(
        AssetId assetId,
        string site,
        bool current,
        DateOnly arrivalDate)
    {
        Location location = new(
            assetId,
            LocationCategory.CorporateOffice, 
            site,
            string.Empty,
            string.Empty,
            current,
            arrivalDate);

        return location;
    }

    public static Location CreatePrivateResidenceLocationRecord(
        AssetId assetId,
        bool current,
        DateOnly arrivalDate)
    {
        Location location = new(
            assetId,
            LocationCategory.PrivateResidence, 
            string.Empty,
            string.Empty,
            string.Empty,
            current,
            arrivalDate);

        return location;
    }

    public void SetDepartureDate(DateOnly departureDate) => DepartureDate = departureDate;
}
