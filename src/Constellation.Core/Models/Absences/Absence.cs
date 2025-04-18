﻿namespace Constellation.Core.Models.Absences;

using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using DomainEvents;
using Identifiers;
using Primitives;
using Shared;
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
        StudentId studentId,
        OfferingId offeringId,
        DateOnly date,
        string periodName,
        string periodTimeframe,
        AbsenceReason absenceReason,
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
        AbsenceTimeframe = $"{startTime:hh:mm tt} - {endTime:hh:mm tt}";
        AbsenceReason = absenceReason;
        StartTime = startTime;
        EndTime = endTime;
        FirstSeen = DateTime.Now;
    }

    public AbsenceId Id { get; private set; }
    public AbsenceType Type { get; private set; }

    public StudentId StudentId { get; private set; }
    public OfferingId OfferingId { get; private set; }

    public DateOnly Date { get; private set; }
    public string PeriodName { get; private set; }
    public string PeriodTimeframe { get; private set; }

    public int AbsenceLength { get; private set; }
    public string AbsenceTimeframe { get; private set; }
    public AbsenceReason AbsenceReason { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    public IReadOnlyList<Notification> Notifications => _notifications;
    public IReadOnlyList<Response> Responses => _responses;

    public bool Explained { get; private set; }
    public DateTime FirstSeen { get; private set; }
    public DateTime LastSeen { get; private set; }

    public static Absence Create(
        AbsenceType type,
        StudentId studentId,
        OfferingId offeringId,
        DateOnly date,
        string periodName,
        string periodTimeframe,
        AbsenceReason absenceReason,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        Absence absence = new(
            new AbsenceId(),
            type,
            studentId,
            offeringId,
            date,
            periodName,
            periodTimeframe,
            absenceReason,
            startTime,
            endTime)
        {
            FirstSeen = DateTime.Now,
            LastSeen = DateTime.Now
        };

        return absence;
    }

    public Result<Notification> AddNotification(
        NotificationType type,
        string message,
        string recipients)
    {
        Notification notification = Notification.Create(Id, type, message, recipients);
        
        _notifications.Add(notification);

        return notification;
    }

    public Result<Notification> AddNotification(
        NotificationType type,
        string message,
        string recipients,
        string messageId,
        DateTime sentAt)
    {
        Notification notification = Notification.Create(Id, type, message, recipients);

        notification.UpdateSentFields(messageId, sentAt);

        _notifications.Add(notification);

        return notification;
    }

    public Result<Response> AddResponse(
        ResponseType type,
        string from,
        string explanation)
    {
        Response response = Response.Create(Id, DateTime.Now, type, from, explanation);

        if (response.VerificationStatus == ResponseVerificationStatus.Pending)
            RaiseDomainEvent(new PendingVerificationResponseCreatedDomainEvent(new DomainEventId(), response.Id, Id));

        if (response.VerificationStatus == ResponseVerificationStatus.NotRequired)
        {
            RaiseDomainEvent(new AbsenceResponseReceivedDomainEvent(new DomainEventId(), response.Id, Id));

            Explained = true;
        }

        _responses.Add(response);

        return response;
    }

    public Response? GetExplainedResponse()
    {
        if (_responses.Count == 0)
            return null;

        //if (!Explained)
        //    return null;

        Response? explainedResponse = _responses
            .FirstOrDefault(response => 
                response.VerificationStatus.Equals(ResponseVerificationStatus.NotRequired));

        if (explainedResponse is not null)
            return explainedResponse;

        Response? verifiedResponse = _responses
            .FirstOrDefault(response =>
                response.VerificationStatus.Equals(ResponseVerificationStatus.Verified));

        if (verifiedResponse is not null)
            return verifiedResponse;

        Response? rejectedResponse = _responses
            .FirstOrDefault(response =>
                response.VerificationStatus.Equals(ResponseVerificationStatus.Rejected));

        if (rejectedResponse is not null)
            return rejectedResponse;

        Response? pendingResponse = _responses
            .FirstOrDefault(response =>
                response.VerificationStatus.Equals(ResponseVerificationStatus.Pending));

        return pendingResponse ?? _responses.First();
    }

    public void ResponseConfirmed(AbsenceResponseId responseId)
    {
        Response response = _responses.First(response => response.Id == responseId);

        if (response.VerificationStatus == ResponseVerificationStatus.Verified ||
            response.VerificationStatus == ResponseVerificationStatus.Rejected)
        {
            RaiseDomainEvent(new AbsenceResponseConfirmedDomainEvent(
                new DomainEventId(),
                responseId,
                Id));

            Explained = true;
        }
    }

    public void MergeAbsence(Absence other)
    {
        AbsenceReason = FindWorstAbsenceReason(new List<AbsenceReason> { AbsenceReason, other.AbsenceReason });

        EndTime = other.EndTime;
        AbsenceTimeframe = $"{AbsenceTimeframe.Split("- ")[0]} - {other.AbsenceTimeframe.Split(" - ")[1]}";
        AbsenceLength += other.AbsenceLength;
    }

    public void UpdateAbsenceReason(AbsenceReason newReason) =>
        AbsenceReason = newReason;

    public void UpdateLastSeen()
    {
        LastSeen = DateTime.Now;
    }

    public static AbsenceReason FindWorstAbsenceReason(IReadOnlyList<AbsenceReason> reasons)
    {
        if (reasons.Any(reason => reason == AbsenceReason.Unjustified))
            return AbsenceReason.Unjustified;

        if (reasons.Any(reason => reason == AbsenceReason.Absent))
            return AbsenceReason.Absent;

        if (reasons.Any(reason => reason == AbsenceReason.Suspended))
            return AbsenceReason.Suspended;

        if (reasons.Any(reason => reason == AbsenceReason.Exempt))
            return AbsenceReason.Exempt;

        if (reasons.Any(reason => reason == AbsenceReason.Leave))
            return AbsenceReason.Leave;

        if (reasons.Any(reason => reason == AbsenceReason.Flexible))
            return AbsenceReason.Flexible;

        if (reasons.Any(reason => reason == AbsenceReason.Sick))
            return AbsenceReason.Sick;

        if (reasons.Any(reason => reason == AbsenceReason.SchoolBusiness))
            return AbsenceReason.SchoolBusiness;

        if (reasons.Any(reason => reason == AbsenceReason.SharedEnrolment))
            return AbsenceReason.SharedEnrolment;

        return AbsenceReason.Unjustified;
    }
}