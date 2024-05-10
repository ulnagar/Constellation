namespace Constellation.Core.Models.Assets;

using Enums;
using Identifiers;
using Primitives;
using System;

public abstract class Allocation : IAuditableEntity
{
    public AllocationId Id { get; protected set; } = new();
    public AssetId AssetId { get; protected set; }
    public LocationCategory Category { get; protected set; }
    public string Site { get; protected set; }
    public string Room { get; protected set; }
    public string ResponsibleOfficer { get; protected set; }
    public bool CurrentLocation { get; protected set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}

public sealed class StudentAllocation : Allocation
{
    public string StudentId { get; private set; }
}

public sealed class StaffAllocation : Allocation
{
    public string StaffId { get; private set; }
}

public sealed class SchoolAllocation : Allocation
{
    public string SchoolCode { get; private set; }
}

public sealed class CommunityAllocation : Allocation
{
    public string EmailAddress { get; private set; }
}