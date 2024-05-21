#nullable enable
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
        string userId,
        string officer)
    {
        AssetId = assetId;
        UserId = userId;
        ResponsibleOfficer = officer;
    }

    public AllocationId Id { get; private set; } = new();
    public AssetId AssetId { get; private set; }
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
        Student student)
    {
        Allocation allocation = new(
            assetId,
            student.StudentId, 
            student.GetName().DisplayName);

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        Staff staffMember)
    {
        Allocation allocation = new(
            assetId,
            staffMember.StaffId, 
            staffMember.GetName()?.DisplayName ?? staffMember.StaffId);

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        School school)
    {
        Allocation allocation = new(
            assetId,
            school.Code,
            school.Name);

        return allocation;
    }

    public static Result<Allocation> Create(
        AssetId assetId,
        EmailRecipient recipient)
    {
        Allocation allocation = new(
            assetId,
            recipient.Email,
            recipient.Name);

        return allocation;
    }
}