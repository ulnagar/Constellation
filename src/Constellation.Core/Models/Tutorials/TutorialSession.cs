namespace Constellation.Core.Models.Tutorials;

using Identifiers;
using Primitives;
using StaffMembers.Identifiers;
using System;
using Timetables.Identifiers;

public sealed class TutorialSession : IAuditableEntity
{
    internal TutorialSession(
        PeriodId periodId,
        StaffId staffId)
    {
        Id = new();

        PeriodId = periodId;
        StaffId = staffId;
    }

    public TutorialSessionId Id { get; private set; }
    public TutorialId TutorialId { get; private set; }
    public PeriodId PeriodId { get; private set; }
    public StaffId StaffId { get; private set; }
    public bool IsDeleted { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    internal void Delete() => IsDeleted = true;
}