namespace Constellation.Core.Models.Offerings;

using Identifiers;
using ValueObjects;
using Primitives;
using System;

public sealed class TeacherAssignment : IAuditableEntity
{
    public TeacherAssignment(
        OfferingId offeringId,
        string staffId,
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

    public string StaffId { get; private set; }

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
