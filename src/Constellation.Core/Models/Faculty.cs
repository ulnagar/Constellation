namespace Constellation.Core.Models;

using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;

public class Faculty : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Colour { get; set; }
    public List<FacultyMembership> Members { get; set; }
}
