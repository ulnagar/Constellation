namespace Constellation.Core.Models.Awards;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Data;

public class StudentAward : AggregateRoot
{
    public const string Astra = "Astra Award";
    public const string Stellar = "Stellar Award";
    public const string Galaxy = "Galaxy Medal";
    public const string Universal = "Aurora Universal Achiever";

    private StudentAward(
        StudentAwardId id,
        string studentId,
        string category,
        string type,
        DateTime awardedOn)
    {
        Id = id;
        StudentId = studentId;
        Category = category;
        Type = type;
        AwardedOn = awardedOn;
    }

    public StudentAwardId Id { get; private set; }
    public string StudentId { get; private set; }
    public string TeacherId { get; private set; }
    public DateTime AwardedOn { get; private set; }
    public string Category { get; private set; }
    public string Type { get; private set; }
    public string IncidentId { get; private set; }
    public string Reason { get; private set; }

    public static StudentAward Create(
        StudentAwardId id,
        string studentId,
        string category,
        string type,
        DateTime awardedOn)
    {
        StudentAward award = new(
            id,
            studentId,
            category,
            type,
            awardedOn);

        award.RaiseDomainEvent(new AwardCreatedDomainEvent(new DomainEventId(), id));

        return award;
    }

    public static StudentAward Create(
        StudentAwardId id,
        DateTime awardedOn,
        string incidentId,
        string teacherId,
        string reason,
        string studentId)
    {
        StudentAward award = new(
            id,
            studentId,
            "Astra Award",
            "Astra Award",
            awardedOn)
        {
            IncidentId = incidentId,
            TeacherId = teacherId,
            Reason = reason
        };

        award.RaiseDomainEvent(new AwardCreatedDomainEvent(new DomainEventId(), id));

        return award;
    }

    public void Update(
        string incidentId,
        string teacherId,
        string reason)
    {
        IncidentId = incidentId;
        TeacherId = teacherId;
        Reason = reason;
    }
}
