namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using System;

public class ClassSession : BaseAuditableEntity
{
    public Guid ClassId { get; set; }
    public virtual Class Class { get; set; }

    public Guid PeriodId { get; set; }
    public virtual Period Period { get; set; }

    public Guid StaffMemberId { get; set; }
    public virtual StaffMember StaffMember { get; set; }
}
