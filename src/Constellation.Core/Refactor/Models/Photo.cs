namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using System;

public abstract class Photo : BaseEntity
{
    public byte[] Contents { get; set; }
}

public class StudentPhoto : Photo
{
    public Guid StudentId { get; set; }
    public virtual Student Student { get; set; }
}

public class StaffPhoto : Photo
{
    public Guid StaffMemberId { get; set; }
    public virtual StaffMember StaffMember { get; set; }
}