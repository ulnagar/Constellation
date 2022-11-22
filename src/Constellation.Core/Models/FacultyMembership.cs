namespace Constellation.Core.Models;

using Constellation.Core.Common;
using Constellation.Core.Enums;
using System;

public class FacultyMembership : AuditableEntity
{
    public Guid Id { get; set; }
    public string StaffId { get; set; }
    public virtual Staff Staff { get; set; }
    public Guid FacultyId { get; set; }
    public virtual Faculty Faculty { get; set; }
    public FacultyMembershipRole Role { get; set; }
}
