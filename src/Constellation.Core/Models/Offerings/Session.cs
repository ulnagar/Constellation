namespace Constellation.Core.Models.Offerings;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;

public sealed class Session : IAuditableEntity
{
    internal Session(
        OfferingId offeringId,
        int periodId)
    {
        OfferingId = offeringId;
        PeriodId = periodId;
    }

    public SessionId Id { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public Offering Offering { get; private set; }
    public int PeriodId { get; private set; }
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