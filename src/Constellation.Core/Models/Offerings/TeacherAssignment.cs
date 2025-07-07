namespace Constellation.Core.Models.Offerings;

using Identifiers;
using Primitives;
using StaffMembers.Identifiers;
using System;
using ValueObjects;

public sealed class TeacherAssignment : IAuditableEntity
{
    public TeacherAssignment(
        OfferingId offeringId,
        StaffId staffId,
        AssignmentType type)
    {
        Id = new();
        OfferingId = offeringId;
        StaffId = staffId;
        Type = type;
    }

    public AssignmentId Id { get; private set; }
    
    public OfferingId OfferingId { get; private set; }
    public Offering Offering { get; private set; }

    public StaffId StaffId { get; private set; }

    public AssignmentType Type { get; private set; }

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }

    public bool IsDeleted { get; private set; }

    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    internal void Delete()
    {
        IsDeleted = true;
    }
}
