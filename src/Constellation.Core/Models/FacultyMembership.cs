namespace Constellation.Core.Models;

using Constellation.Core.Enums;
using Constellation.Core.Primitives;
using System;

public class FacultyMembership : IAuditableEntity
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public virtual Staff Staff { get; set; }
    public Guid FacultyId { get; set; }
    public virtual Faculty Faculty { get; set; }
    public FacultyMembershipRole Role { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
