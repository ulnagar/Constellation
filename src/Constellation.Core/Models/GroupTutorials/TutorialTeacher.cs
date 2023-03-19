namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using System;

public sealed class TutorialTeacher : IAuditableEntity
{
    public TutorialTeacher(
        TutorialTeacherId id,
        Staff staff,
        DateOnly? effectiveTo)
    {
        Id = id;
        StaffId = staff.StaffId;
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
