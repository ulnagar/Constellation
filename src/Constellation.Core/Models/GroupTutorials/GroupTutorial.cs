namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;

public sealed class GroupTutorial : AuditableEntity
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Staff> Teachers { get; set; } = new();
    public List<TutorialEnrolment> Enrolments { get; set; } = new();
    public List<TutorialRoll> Rolls { get; set; }
}