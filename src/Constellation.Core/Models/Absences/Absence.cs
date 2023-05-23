namespace Constellation.Core.Models.Absences;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public class Absence : AggregateRoot
{
    private readonly List<Notification> _notifications = new();
    private readonly List<Response> _responses = new();

    private Absence(
        AbsenceId id,
        AbsenceType type,
        string studentId,
        int offeringId,
        DateOnly date,
        string periodName,
        string periodTimeframe,
        string absenceReason,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        Id = id;
        Type = type;
        StudentId = studentId;
        OfferingId = offeringId;
        Date = date;
        PeriodName = periodName;
        PeriodTimeframe = periodTimeframe;
        AbsenceLength = (int)(endTime - startTime).TotalMinutes;
        AbsenceTimeframe = $"{startTime:hh:MM tt} - {endTime:hh:MM tt}";
        AbsenceReason = absenceReason;
        StartTime = startTime;
        EndTime = endTime;
        FirstSeen = DateTime.Now;
    }

    public AbsenceId Id { get; private set; }
    public AbsenceType Type { get; private set; }

    public string StudentId { get; private set; }
    public int OfferingId { get; private set; }

    public DateOnly Date { get; private set; }
    public string PeriodName { get; private set; }
    public string PeriodTimeframe { get; private set; }

    public int AbsenceLength { get; private set; }
    public string AbsenceTimeframe { get; private set; }
    public string AbsenceReason { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public IReadOnlyList<Notification> Notifications => _notifications;
    public IReadOnlyList<Response> Responses => _responses;

    public bool Explained => Responses.Any(response =>
        response.Type == ResponseType.Student && response.VerificationStatus == ResponseVerificationStatus.Verified ||
        response.Type == ResponseType.Parent || 
        response.Type == ResponseType.Coordinator ||
        response.Type == ResponseType.System);

    public DateTime FirstSeen { get; private set; }
    public DateTime LastSeen { get; private set; }

    public static Absence Create(
        AbsenceType type,
        string studentId,
        int offeringId,
        DateOnly date,
        string periodName,
        string periodTimeframe,
        string absenceReason,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        var absence = new Absence(
            new AbsenceId(),
            type,
            studentId,
            offeringId,
            date,
            periodName,
            periodTimeframe,
            absenceReason,
            startTime,
            endTime);

        return absence;
    }

    public Result<Notification> AddNotification(
        NotificationType type,
        string message,
        string recipients)
    {
        var notification = Notification.Create(Id, type, message, recipients);

        _notifications.Add(notification);

        return notification;
    }

    public Result<Response> AddResponse(
        ResponseType type,
        string from,
        string explanation)
    {
        var response = Response.Create(Id, DateTime.Now, type, from, explanation);

        if (response.VerificationStatus == ResponseVerificationStatus.Pending)
            RaiseDomainEvent(new PendingVerificationResponseCreatedDomainEvent(new DomainEventId(), response.Id, Id));

        _responses.Add(response);

        return response;
    }
}