namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;
using System;

public sealed class TutorialEnrolment : Entity, IAuditableEntity
{
    public TutorialEnrolment(Guid id, Student student, DateTime? effectiveTo)
        : base(id)
    {
        StudentId = student.StudentId;
        EffectiveFrom = DateTime.Today;
        EffectiveTo = effectiveTo;
    }

    public string StudentId { get; set; }
    public Student Student { get; set; }
    public Guid TutorialId { get; set; }
    public GroupTutorial Tutorial { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
