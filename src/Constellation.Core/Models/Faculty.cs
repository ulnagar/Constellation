namespace Constellation.Core.Models;

using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;

public class Faculty : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Colour { get; set; }
    public List<FacultyMembership> Members { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
