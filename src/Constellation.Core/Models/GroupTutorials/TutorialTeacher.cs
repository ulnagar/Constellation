namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;

public sealed class TutorialTeacher : Entity, IAuditableEntity
{
    private TutorialTeacher() { }

    public TutorialTeacher(Guid id, Staff staff, DateTime? effectiveTo)
        : base(id)
    {
        StaffId = staff.StaffId;
        EffectiveFrom = DateTime.Today;
        EffectiveTo = effectiveTo;
    }

    public string StaffId { get; set; } = string.Empty;
    public Guid TutorialId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
