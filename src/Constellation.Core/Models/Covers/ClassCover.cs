namespace Constellation.Core.Models.Covers;

using Constellation.Core.Models.Offerings.Identifiers;
using DomainEvents;
using Identifiers;
using Primitives;
using System;
using ValueObjects;

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
        ClassCover cover = new(id, offeringId, startDate, endDate, teacherType, teacherId);

        cover.RaiseDomainEvent(new CoverCreatedDomainEvent(new(), cover.Id));

        return cover;
    }

    public void EditDates(DateOnly startDate, DateOnly endDate)
    {
        DateOnly oldStartDate = StartDate;
        DateOnly oldEndDate = EndDate;

        StartDate = startDate;
        EndDate = endDate;

        if (oldStartDate != StartDate && oldEndDate != EndDate)
        {
            RaiseDomainEvent(new CoverStartAndEndDatesChangedDomainEvent(new (), Id, oldStartDate, oldEndDate, StartDate, EndDate));

            return;
        } 

        if (oldStartDate != StartDate)
        {
            RaiseDomainEvent(new CoverStartDateChangedDomainEvent(new (), Id, oldStartDate, StartDate));

            return;
        } 

        if (oldEndDate != EndDate)
        {
            RaiseDomainEvent(new CoverEndDateChangedDomainEvent(new (), Id, oldEndDate, EndDate));

            return;
        }
    }

    public void Delete()
    {
        IsDeleted = true;

        RaiseDomainEvent(new CoverCancelledDomainEvent(new (), Id));
    }
}