﻿namespace Constellation.Core.Models.GroupTutorials;

using Constellation.Core.Primitives;
using System;

public sealed class TutorialTeacher : Entity, IAuditableEntity
{
    private TutorialTeacher() { }

    public TutorialTeacher(Guid id, Staff staff, DateOnly? effectiveTo)
        : base(id)
    {
        StaffId = staff.StaffId;
        EffectiveFrom = DateOnly.FromDateTime(DateTime.Today);
        EffectiveTo = effectiveTo;
    }

    public string StaffId { get; set; }
    public Guid TutorialId { get; set; }
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