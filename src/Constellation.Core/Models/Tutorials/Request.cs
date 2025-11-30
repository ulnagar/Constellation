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
    private readonly List<RequestNote> _notes = [];

    private Request() { }

    private Request(
        StudentId studentId,
        Name student,
        Grade grade,
        string school,
        TutorialType type,
        string subject,
        List<PeriodId> periodIds,
        string justification)
    {
        Id = new();

        StudentId = studentId;
        Student = student;
        Grade = grade;
        School = school;
        Type = type;
        Subject = subject;
        _periodIds.AddRange(periodIds);
        Justification = justification;

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
    public string Justification { get; private set; }
    public RequestStatus Status { get; private set; }
    public IReadOnlyList<RequestNote> Notes => _notes.AsReadOnly();
    public RequestPlan? Plan { get; private set; }

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
        List<Period> periods,
        string justification)
    {
        Request request = new(
            student.Id,
            student.Name,
            student.CurrentEnrolment?.Grade ?? Core.Enums.Grade.SpecialProgram,
            student.CurrentEnrolment?.SchoolName ?? string.Empty,
            type,
            subject,
            periods.Select(period => period.Id).ToList(),
            justification);

        request.RaiseDomainEvent(new TutorialRequestCreatedDomainEvent(new(), request.Id));

        return request;
    }

    public Result Review(
        RequestStatus newStatus,
        string message,
        string reviewer,
        IDateTimeProvider dateTime)
    {
        if (newStatus == RequestStatus.Requested || 
            newStatus == RequestStatus.Approved && Status != RequestStatus.Requested || 
            newStatus == RequestStatus.Scheduled)
            return Result.Failure(TutorialRequestErrors.InvalidStatus);

        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure(TutorialRequestErrors.MustIncludeNote);

        Status = newStatus;

        RequestNoteAction action = newStatus switch
        {
            _ when newStatus == RequestStatus.Approved => RequestNoteAction.Approved,
            _ when newStatus == RequestStatus.Rejected => RequestNoteAction.Rejected,
            _ when newStatus == RequestStatus.Scheduled => RequestNoteAction.Scheduled,
            _ => RequestNoteAction.Note
        };

        RequestNote note = RequestNote.Create(Id, message, action, reviewer, dateTime.Now);
        _notes.Add(note);

        if (newStatus == RequestStatus.Approved)
            RaiseDomainEvent(new TutorialRequestApprovedDomainEvent(new(), Id));

        if (newStatus == RequestStatus.Rejected)
            RaiseDomainEvent(new TutorialRequestRejectedDomainEvent(new(), Id));

        if (newStatus == RequestStatus.Scheduled)
            RaiseDomainEvent(new TutorialRequestScheduledDomainEvent(new(), Id));

        return Result.Success();
    }

    public Result ScheduleRequest(
        RequestPlan plan,
        string message,
        string reviewer,
        IDateTimeProvider dateTime)
    {
        if (Status != RequestStatus.Approved)
            return Result.Failure(TutorialRequestErrors.InvalidStatus);

        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure(TutorialRequestErrors.MustIncludeNote);

        Status = RequestStatus.Scheduled;
        
        RequestNote note = RequestNote.Create(Id, message, RequestNoteAction.Scheduled, reviewer, dateTime.Now);
        _notes.Add(note);

        RaiseDomainEvent(new TutorialRequestScheduledDomainEvent(new(), Id));

        Plan = plan;

        return Result.Success();
    }

    public void AddNote(
        string message,
        string username,
        IDateTimeProvider dateTime)
    {
        RequestNote note = RequestNote.Create(Id, message, RequestNoteAction.Note, username, dateTime.Now);
        _notes.Add(note);
    }

    public void Delete() => IsDeleted = true;
}