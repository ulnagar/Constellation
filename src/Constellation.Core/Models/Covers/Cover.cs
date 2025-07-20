namespace Constellation.Core.Models.Covers;

using Constellation.Core.Models.Covers.Enums;
using Constellation.Core.Models.Covers.Identifiers;
using Events;
using Offerings.Identifiers;
using Primitives;
using System;

public abstract class Cover : AggregateRoot, IAuditableEntity
{
    public CoverId Id { get; protected init; }
    public OfferingId OfferingId { get; protected init; }
    public DateOnly StartDate { get; protected set; }
    public DateOnly EndDate { get; protected set; }
    public CoverTeacherType TeacherType { get; protected init; }
    public string TeacherId { get; protected init; }


    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public void EditDates(DateOnly startDate, DateOnly endDate)
    {
        DateOnly oldStartDate = StartDate;
        DateOnly oldEndDate = EndDate;

        StartDate = startDate;
        EndDate = endDate;

        if (oldStartDate != StartDate && oldEndDate != EndDate)
        {
            RaiseDomainEvent(new CoverStartAndEndDatesChangedDomainEvent(new(), Id, oldStartDate, oldEndDate, StartDate, EndDate));

            return;
        }

        if (oldStartDate != StartDate)
        {
            RaiseDomainEvent(new CoverStartDateChangedDomainEvent(new(), Id, oldStartDate, StartDate));

            return;
        }

        if (oldEndDate != EndDate)
        {
            RaiseDomainEvent(new CoverEndDateChangedDomainEvent(new(), Id, oldEndDate, EndDate));

            return;
        }
    }

    public void Delete()
    {
        IsDeleted = true;

        RaiseDomainEvent(new CoverCancelledDomainEvent(new(), Id));
    }
}