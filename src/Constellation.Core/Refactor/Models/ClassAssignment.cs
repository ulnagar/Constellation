namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System;

public class ClassAssignment : BaseAuditableEntity
{
    public Guid ClassId { get; set; }
    public virtual Class Class { get; set; }

    public Guid StaffMemberId { get; set; }
    public virtual StaffMember StaffMember { get; set; }

    public TeacherType TeacherType { get; set; }
}