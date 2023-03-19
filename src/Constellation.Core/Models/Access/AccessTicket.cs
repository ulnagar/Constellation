namespace Constellation.Core.Models.Access;

using Constellation.Core.Primitives;
using System;

public sealed class AccessTicket : AggregateRoot, IAuditableEntity
{
	public AccessTicket(Guid Id)
	{
	}

	public Guid Id { get; set; }
	public string AuthoriserType { get; set; } // Convert to Enumeration
	public string AuthoriserId { get; set; } // Convert to GUID once all other objects have been converted to GUID ID
	public string SystemType { get; set; } // Convert to Enumeration
	public string SystemObjectId { get; set; }
	public string SystemAccessLevel { get; set; }
	public string PrincipalType { get; set; } // Convert to Enumeration
	public string PrincipalId { get; set; } // Convert to GUID once all other objects have been converted to GUID ID
	public DateOnly ValidFrom { get; set; }
	public DateOnly ValidUntil { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
