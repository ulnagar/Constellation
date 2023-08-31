namespace Constellation.Core.Models.Subjects;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;

public sealed class Course : AggregateRoot
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public Grade Grade { get; set; }
    public Guid FacultyId { get; set; }
    public Faculty Faculty { get; set; }
    public decimal FullTimeEquivalentValue { get; set; }
    public List<Offering> Offerings { get; set; } = new();
}