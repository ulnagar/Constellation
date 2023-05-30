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

        if (response.VerificationStatus == ResponseVerificationStatus.NotRequired)
            RaiseDomainEvent(new AbsenceResponseReceivedDomainEvent(new DomainEventId(), response.Id, Id));

        _responses.Add(response);

        return response;
    }

    public Response? GetExplainedResponse()
    {
        if (_responses.Count == 0)
            return null;

        var explainedResponse = _responses
            .FirstOrDefault(response => 
                response.VerificationStatus == ResponseVerificationStatus.NotRequired);

        if (explainedResponse is not null)
            return explainedResponse;

        var verifiedResponse = _responses
            .FirstOrDefault(response =>
                response.VerificationStatus == ResponseVerificationStatus.Verified);

        if (verifiedResponse is not null)
            return verifiedResponse;

        var rejectedResponse = _responses
            .FirstOrDefault(response =>
                response.VerificationStatus == ResponseVerificationStatus.Rejected);

        if (rejectedResponse is not null)
            return rejectedResponse;

        var pendingResponse = _responses
            .FirstOrDefault(response =>
                response.VerificationStatus == ResponseVerificationStatus.Pending);

        if (pendingResponse is not null)
            return pendingResponse;

        return _responses.First();
    }

    public void ResponseConfirmed(AbsenceResponseId responseId)
    {
        var response = _responses.First(response => response.Id == responseId);

        if (response.VerificationStatus == ResponseVerificationStatus.Verified ||
            response.VerificationStatus == ResponseVerificationStatus.Rejected)
        {
            RaiseDomainEvent(new AbsenceResponseConfirmedDomainEvent(
                new DomainEventId(),
                responseId,
                Id));
        }
    }
}