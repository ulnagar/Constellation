namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Models.Students.Identifiers;
using Identifiers;
using Primitives;
using Students;
using System;

public sealed class TutorialEnrolment : IAuditableEntity
{
    // Private ctor needed to allow EFCore to create entity
    private TutorialEnrolment() { }

    public TutorialEnrolment(
        TutorialEnrolmentId id,
        Student student,
        DateOnly? effectiveTo)
    {
        Id = id;
        StudentId = student.Id;
        EffectiveFrom = DateOnly.FromDateTime(DateTime.Today);
        EffectiveTo = effectiveTo;
    }

    public TutorialEnrolmentId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public GroupTutorialId TutorialId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
