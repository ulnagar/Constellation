namespace Constellation.Core.Models.MissedWork;

using Constellation.Core.DomainEvents;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public class ClassworkNotification : AggregateRoot
{
    private readonly List<Absence> _absences = new();
    private readonly List<Staff> _teachers = new();

    private ClassworkNotification(
        ClassworkNotificationId id,
        List<Absence> absences,
        List<Staff> teachers,
        bool isCovered)
    {
        Id = id;
        GeneratedAt = DateTime.Now;
        _absences = absences;
        _teachers = teachers;
        IsCovered = isCovered;

        OfferingId = absences.First().OfferingId;
        AbsenceDate = absences.First().Date;
    }

    public ClassworkNotificationId Id { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    // Absence fields
    public IReadOnlyList<Absence> Absences => _absences;
    public IReadOnlyList<Staff> Teachers => _teachers;
    public int OfferingId { get; private set; }
    public DateOnly AbsenceDate { get; private set; }
    public bool IsCovered { get; private set; }

    // Completion fields
    public string Description { get; private set; }
    public string CompletedBy { get; private set; }
    public string StaffId { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public static Result<ClassworkNotification> Create(
        List<Absence> absences,
        List<Staff> teachers,
        bool isCovered)
    {
        if (!absences.Any())
            return Result.Failure<ClassworkNotification>(DomainErrors.MissedWork.ClassworkNotification.RequireAbsenceToCreate);

        if (!teachers.Any())
            return Result.Failure<ClassworkNotification>(DomainErrors.MissedWork.ClassworkNotification.RequireTeacherToCreate);

        var entity = new ClassworkNotification(
            new ClassworkNotificationId(),
            absences,
            teachers,
            isCovered);

        entity.RaiseDomainEvent(new ClassworkNotificationCreatedDomainEvent(new DomainEventId(), entity.Id));

        return entity;
    }

    // Use this method to split a Classwork Notification and create a
    // new entry for some of the student from the original
    // Must remove the split students from the original notification
    public Result<ClassworkNotification> Split(
        List<Absence> absences)
    {
        if (!string.IsNullOrWhiteSpace(Description))
            return Result.Failure<ClassworkNotification>(DomainErrors.MissedWork.ClassworkNotification.SplitCompletedNotification);

        if (!absences.Any())
            return Result.Failure<ClassworkNotification>(DomainErrors.MissedWork.ClassworkNotification.RequireAbsenceToCreate);

        foreach (var absence in absences)
            _absences.Remove(absence);

        var newEntity = new ClassworkNotification(
            new ClassworkNotificationId(),
            absences,
            _teachers,
            IsCovered);

        newEntity.RaiseDomainEvent(new ClassworkNotificationSplitDomainEvent(new DomainEventId(), Id, newEntity.Id));

        return newEntity;
    }

    public void AddAbsence(
        Absence absence)
    {
        if (_absences.Contains(absence))
            return;

        if (CompletedAt.HasValue)
            return;

        _absences.Add(absence);
    }

    public void RemoveAbsence(
        Absence absence)
    {
        if (CompletedAt.HasValue)
            return;

        if (_absences.Contains(absence))
            _absences.Remove(absence);
    }

    public Result RecordResponse(
        string description,
        string completedBy,
        string staffId)
    {
        if (!string.IsNullOrWhiteSpace(Description))
            return Result.Failure(DomainErrors.MissedWork.ClassworkNotification.AlreadyCompleted);

        Description = description;
        CompletedBy = completedBy;
        CompletedAt = DateTime.Now;
        StaffId = staffId;

        RaiseDomainEvent(new ClassworkNotificationCompletedDomainEvent(new DomainEventId(), Id));

        return Result.Success();
    }
}
