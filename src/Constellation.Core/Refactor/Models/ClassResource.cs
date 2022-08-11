namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System;

public abstract class SystemResource : BaseAuditableEntity
{
    public ResourceType ResourceType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string ResourceLinkId { get; set; }
}

public class ClassResource : SystemResource
{
    public Guid ClassId { get; set; }
    public virtual Class Class { get; set; }
}

public class FacultyResource : SystemResource
{
    public Guid FacultyId { get; set; }
    public virtual Faculty Faculty { get; set; }
}

public class GradeResource : SystemResource
{
    public Guid GradeId { get; set; }
    public virtual Grade Grade { get; set; }
}