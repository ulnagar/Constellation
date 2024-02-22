namespace Constellation.Core.Models.GroupTutorials;

using Identifiers;
using Primitives;
using System;

public sealed class TutorialTeacher : IAuditableEntity
{
    // Private ctor needed to allow EFCore to create entity
    //private TutorialTeacher() { }

    public TutorialTeacher(
        TutorialTeacherId id,
        string staffId,
        DateOnly? effectiveTo)
    {
        Id = id;
        StaffId = staffId;
        EffectiveFrom = DateOnly.FromDateTime(DateTime.Today);
        EffectiveTo = effectiveTo;
    }

    public TutorialTeacherId Id { get; private set; }
    public string StaffId { get; private set; }
    public GroupTutorialId TutorialId { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; internal set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; internal set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }
}
