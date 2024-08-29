#nullable enable
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Assets.Enums;

namespace Constellation.Core.Models.Assets;

using Core.ValueObjects;
using Identifiers;
using Primitives;
using Shared;
using Students;
using System;

public class Allocation : IAuditableEntity
{
    // Required by EF Core
    private Allocation() { }

    private Allocation(
        AssetId assetId,
        AllocationType allocationType,
        string userId,
        string officer)
    {
        AssetId = assetId;
        AllocationType = allocationType;
        UserId = userId;
        ResponsibleOfficer = officer;
    }

    public AllocationId Id { get; private set; } = new();
    public AssetId AssetId { get; private set; }
    public AllocationType AllocationType { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string ResponsibleOfficer { get; private set; } = string.Empty;

    public DateOnly AllocationDate { get; private set; }
    public DateOnly ReturnDate { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }

    public static Result<Allocation> Create(
        AssetId assetId,
        Student student,
        DateOnly allocatedOn)
    {
        Allocation allocation = new(
            assetId,
            AllocationType.Student,
            student.Id.ToString(), 
            student.Name.DisplayName);

        allocation.SetAllocationDate(allocatedOn);

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        Staff staffMember,
        DateOnly allocatedOn)
    {
        Allocation allocation = new(
            assetId,
            AllocationType.Staff, 
            staffMember.StaffId, 
            staffMember.GetName()?.DisplayName ?? staffMember.StaffId);

        allocation.SetAllocationDate(allocatedOn);

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        School school,
        DateOnly allocatedOn)
    {
        Allocation allocation = new(
            assetId,
            AllocationType.School,
            school.Code,
            school.Name);

        allocation.SetAllocationDate(allocatedOn);

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        EmailRecipient recipient,
        DateOnly allocatedOn)
    {
        Allocation allocation = new(
            assetId,
            AllocationType.CommunityMember, 
            recipient.Email,
            recipient.Name);

        allocation.SetAllocationDate(allocatedOn);

        return allocation;
    }

    internal void Delete(IDateTimeProvider dateTime)
    {
        IsDeleted = true;
        ReturnDate = dateTime.Today;
    }
    
    private void SetAllocationDate(DateOnly allocatedOn) => AllocationDate = allocatedOn;
}