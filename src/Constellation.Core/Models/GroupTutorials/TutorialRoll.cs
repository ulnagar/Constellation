namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;

public sealed class TutorialRoll : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid TutorialId { get; set; }
    public GroupTutorial Tutorial { get; set; }
    public DateTime SessionDate { get; set; }
    public string StaffId { get; set; }
    public Staff Staff { get; set; }
    public List<TutorialRollStudent> Students { get; set; } = new();
}
