namespace Constellation.Core.Refactor.Common;

using System;

public class BaseAuditableEntity : BaseEntity
{
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string LastModifiedBy { get; set; }
    public bool IsDeleted => Deleted.HasValue;
    public DateTime? Deleted { get; set; }
    public string DeletedBy { get; set; }
}