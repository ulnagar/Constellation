namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using System;
using System.Collections.Generic;

public class Faculty : BaseAuditableEntity
{
    public string Name { get; set; }
    
    public Guid FacultyId { get; set; }
    public virtual Faculty ManagementUnit { get; set; }

    public Guid StaffMemberId { get; set; }
    public virtual StaffMember HeadTeacher { get; set; }

    public IList<StaffMember> StaffMembers { get; private set; } = new List<StaffMember>();
    public IList<FacultyResource> Resources { get; private set; } = new List<FacultyResource>();
}
