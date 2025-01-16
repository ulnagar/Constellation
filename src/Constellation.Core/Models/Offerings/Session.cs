namespace Constellation.Core.Models.Offerings;

using Identifiers;
using Primitives;
using System;
using Timetables.Identifiers;

public sealed class Session : IAuditableEntity
{
    internal Session(
        OfferingId offeringId,
        PeriodId periodId)
    {
        Id = new();
        OfferingId = offeringId;
        PeriodId = periodId;
    }

    public SessionId Id { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public Offering Offering { get; private set; }
    public PeriodId PeriodId { get; private set; }
    public bool IsDeleted { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    internal void Delete()
    {
        IsDeleted = true;
    }

}