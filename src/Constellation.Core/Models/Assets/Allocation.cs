namespace Constellation.Core.Models.Assets;

using Core.ValueObjects;
using Enums;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using Students;
using System;

public class Allocation : IAuditableEntity
{
    private Allocation(
        AssetId assetId)
    {
        AssetId = assetId;
    }

    public AllocationId Id { get; private set; } = new();
    public AssetId AssetId { get; private set; }
    public LocationCategory Category { get; private set; }
    public string Site { get; private set; }
    public string SchoolCode { get; private set; }
    public string Room { get; private set; }
    public string UserId { get; private set; }
    public string ResponsibleOfficer { get; private set; }
    public bool CurrentLocation { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Result<Allocation> Create(
        AssetId assetId,
        LocationCategory category,
        string site,
        Student student)
    {
        if (student is null)
            return Result.Failure<Allocation>(AssetErrors.Allocation.StudentEmpty);

        Allocation allocation = new(assetId);

        allocation.Category = category;
        allocation.Site = site;
        allocation.SchoolCode = student.SchoolCode;
        allocation.Room = string.Empty;
        allocation.UserId = student.StudentId;
        allocation.ResponsibleOfficer = student.GetName().DisplayName;

        allocation.CurrentLocation = true;

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        LocationCategory category,
        string site,
        string schoolCode,
        string room,
        Staff staffMember)
    {
        if (staffMember is null)
            return Result.Failure<Allocation>(AssetErrors.Allocation.StaffEmpty);

        Allocation allocation = new(assetId);

        allocation.Category = category;
        allocation.Site = site;
        allocation.SchoolCode = schoolCode;
        allocation.Room = room;
        allocation.UserId = staffMember.StaffId;
        allocation.ResponsibleOfficer = staffMember.GetName().DisplayName;

        allocation.CurrentLocation = true;

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        LocationCategory category,
        string room,
        School school)
    {
        if (school is null)
            return Result.Failure<Allocation>(AssetErrors.Allocation.SchoolEmpty);

        Allocation allocation = new(assetId);

        allocation.Category = category;
        allocation.Site = school.Name;
        allocation.SchoolCode = school.Code;
        allocation.Room = room;
        allocation.UserId = school.Code;
        allocation.ResponsibleOfficer = school.Name;

        allocation.CurrentLocation = true;

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        LocationCategory category,
        string site,
        string room,
        EmailRecipient recipient)
    {
        if (recipient is null)
            return Result.Failure<Allocation>(AssetErrors.Allocation.RecipientEmpty);

        Allocation allocation = new(assetId);

        allocation.Category = category;
        allocation.Site = site;
        allocation.SchoolCode = string.Empty;
        allocation.Room = room;
        allocation.UserId = recipient.Email;
        allocation.ResponsibleOfficer = recipient.Name;

        allocation.CurrentLocation = true;

        return allocation;
    }

    public Result UpdateCurrentFlag(bool current)
    {
        if (current is true && CurrentLocation is false)
            return Result.Failure(AssetErrors.Allocation.CurrentLocation.ReactivationBlocked);

        CurrentLocation = current;

        return Result.Success();
    }

    public void UpdateLocation(
        LocationCategory category,
        string site,
        string schoolCode,
        string room)
    {
        Category = category;
        Site = site;
        SchoolCode = schoolCode;
        Room = room;
    }
}