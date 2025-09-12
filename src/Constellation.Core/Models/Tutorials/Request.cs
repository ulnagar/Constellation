#nullable enable
namespace Constellation.Core.Models.Tutorials;

using Abstractions.Clock;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Enums;
using Core.ValueObjects;
using Enums;
using Events;
using Identifiers;
using Primitives;
using Shared;
using Students;
using Students.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using Timetables;
using Timetables.Identifiers;

public sealed class Request : AggregateRoot, IAuditableEntity
{
    private readonly List<PeriodId> _periodIds = [];

    private Request() { }

    private Request(
        StudentId studentId,
        Name student,
        Grade grade,
        string school,
        TutorialType type,
        string subject,
        List<PeriodId> periodIds)
    {
        Id = new();

        StudentId = studentId;
        Student = student;
        Grade = grade;
        School = school;
        Type = type;
        Subject = subject;
        _periodIds.AddRange(periodIds);

        Status = RequestStatus.Requested;
    }

    public RequestId Id { get; private set; }
    public StudentId StudentId { get; private set; }
    public Name Student { get; private set; }
    public Grade Grade { get; private set; }
    public string School { get; private set; }
    public TutorialType Type { get; private set; }
    public string Subject { get; private set; }
    public IReadOnlyList<PeriodId> PeriodIds => _periodIds.AsReadOnly();
    public RequestStatus Status { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public string ReviewedBy { get; private set; } = string.Empty;
    public DateTime ReviewedAt { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }


    public static Request Create(
        Student student,
        TutorialType type,
        string subject,
        List<Period> periods)
    {
        Request request = new(
            student.Id,
            student.Name,
            student.CurrentEnrolment?.Grade ?? Core.Enums.Grade.SpecialProgram,
            student.CurrentEnrolment?.SchoolName ?? string.Empty,
            type,
            subject,
            periods.Select(period => period.Id).ToList());

        request.RaiseDomainEvent(new TutorialRequestCreatedDomainEvent(new(), request.Id));

        return request;
    }

    public Result Review(
        RequestStatus newStatus,
        string note,
        string reviewer,
        IDateTimeProvider dateTime)
    {
        if (Status != RequestStatus.Requested)
            return Result.Failure(TutorialRequestErrors.AlreadyReviewed);

        if (string.IsNullOrWhiteSpace(note))
            return Result.Failure(TutorialRequestErrors.MustIncludeNote);

        Status = newStatus;
        AddNotes(note);
        ReviewedBy = reviewer;
        ReviewedAt = dateTime.Now;

        if (newStatus == RequestStatus.Approved)
            RaiseDomainEvent(new TutorialRequestApprovedDomainEvent(new(), Id));

        if (newStatus == RequestStatus.Rejected)
            RaiseDomainEvent(new TutorialRequestRejectedDomainEvent(new(), Id));

        return Result.Success();
    }

    public void Delete() => IsDeleted = true;

    public void AddNotes(string notes) => Notes = notes;
}
