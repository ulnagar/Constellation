namespace Constellation.Core.Refactor.Models;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.ValueObjects;
using System;
using System.Collections.Generic;

public class Class : BaseAuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ClassType ClassType { get; set; }
    
    public Guid FacultyId { get; set; }
    public virtual Faculty Faculty { get; set; }

    public Guid GradeId { get; set; }
    public virtual Grade Grade { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public decimal FullTimeEquivalentValue { get; set; }
    public IList<Enrolment> Enrolments { get; private set; } = new List<Enrolment>();
    public IList<ClassSession> Sessions { get; private set; } = new List<ClassSession>();
    public IList<ClassAssignment> Teachers { get; private set; } = new List<ClassAssignment>();
    public IList<ClassResource> Resources { get; private set; } = new List<ClassResource>();
}

public class TutorialClass : Class
{
    protected new decimal FullTimeEquivalentValue = 0;

    public Guid ClassId { get; set; }
    public virtual Class AssociatedClass { get; set; }
}