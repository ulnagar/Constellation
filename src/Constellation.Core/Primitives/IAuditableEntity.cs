namespace Constellation.Core.Primitives;

using System;

public interface IAuditableEntity
{
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
