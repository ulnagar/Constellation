namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;
using System;

public sealed class TutorialEnrolment : AuditableEntity
{
    public Guid Id { get; set; }
    public string StudentId { get; set; }
    public Student Student { get; set; }
    public Guid TutorialId { get; set; }
    public GroupTutorial Tutorial { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
