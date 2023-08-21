namespace Constellation.Core.Models.Subjects;

using Constellation.Core.Enums;
using System;
using System.Collections.Generic;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Grade Grade { get; set; }
    public Guid FacultyId { get; set; }
    public virtual Faculty Faculty { get; set; }
    public decimal FullTimeEquivalentValue { get; set; }
    public List<CourseOffering> Offerings { get; set; } = new();
}