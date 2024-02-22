namespace Constellation.Core.Models.Covers;

using DomainEvents;
using Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Primitives;
using ValueObjects;
using System;

public sealed class ClassCover : AggregateRoot, IAuditableEntity
{
    private ClassCover(
        ClassCoverId id,
        OfferingId offeringId,
        DateOnly startDate,
        DateOnly endDate,
        CoverTeacherType teacherType,
        string teacherId)
    {
        Id = id;
        OfferingId = offeringId;
        StartDate = startDate;
        EndDate = endDate;
        TeacherType = teacherType;
        TeacherId = teacherId;
    }

    public ClassCoverId Id { get; private set; }
    public OfferingId OfferingId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public CoverTeacherType TeacherType {get; private set;}
    public string TeacherId { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static ClassCover Create(
        ClassCoverId id,
        OfferingId offeringId,
        DateOnly startDate,
        DateOnly endDate,
        CoverTeacherType teacherType,
        string teacherId)
    {
        var cover = new ClassCover(id, offeringId, startDate, endDate, teacherType, teacherId);

        cover.RaiseDomainEvent(new CoverCreatedDomainEvent(new DomainEventId(Guid.NewGuid()), cover.Id));

        return cover;
    }

    public void EditDates(DateOnly startDate, DateOnly endDate)
    {
        var oldStartDate = StartDate;
        var oldEndDate = EndDate;

        StartDate = startDate;
        EndDate = endDate;

        if (oldStartDate != StartDate && oldEndDate != EndDate)
        {
            RaiseDomainEvent(new CoverStartAndEndDatesChangedDomainEvent(new DomainEventId(Guid.NewGuid()), Id, oldStartDate, oldEndDate, StartDate, EndDate));

            return;
        } 

        if (oldStartDate != StartDate)
        {
            RaiseDomainEvent(new CoverStartDateChangedDomainEvent(new DomainEventId(Guid.NewGuid()), Id, oldStartDate, StartDate));

            return;
        } 

        if (oldEndDate != EndDate)
        {
            RaiseDomainEvent(new CoverEndDateChangedDomainEvent(new DomainEventId(Guid.NewGuid()), Id, oldEndDate, EndDate));

            return;
        }
    }

    public void Delete()
    {
        IsDeleted = true;

        RaiseDomainEvent(new CoverCancelledDomainEvent(new DomainEventId(Guid.NewGuid()), Id));
    }
}