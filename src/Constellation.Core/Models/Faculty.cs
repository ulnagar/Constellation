namespace Constellation.Core.Models;

using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;

public class Faculty : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
    public List<FacultyMembership> Members { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
