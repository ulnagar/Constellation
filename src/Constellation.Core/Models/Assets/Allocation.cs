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
    public string UserId { get; private set; }
    public string ResponsibleOfficer { get; private set; }

    public DateOnly AllocationDate { get; private set; }
    public DateOnly ReturnDate { get; private set; }

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

        allocation.UserId = student.StudentId;
        allocation.ResponsibleOfficer = student.GetName().DisplayName;
        
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

        allocation.UserId = staffMember.StaffId;
        allocation.ResponsibleOfficer = staffMember.GetName().DisplayName;

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

        allocation.UserId = school.Code;
        allocation.ResponsibleOfficer = school.Name;

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

        allocation.UserId = recipient.Email;
        allocation.ResponsibleOfficer = recipient.Name;
        
        return allocation;
    }
}