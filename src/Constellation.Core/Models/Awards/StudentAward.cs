namespace Constellation.Core.Models.Awards;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Events;
using System;

public class StudentAward : AggregateRoot
{
    public const string Astra = "Astra Award";
    public const string Stellar = "Stellar Award";
    public const string Galaxy = "Galaxy Medal";
    public const string Universal = "Aurora Universal Achiever";

    private StudentAward(
        string studentId,
        string category,
        string type,
        DateTime awardedOn)
    {
        Id = new();
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
        string studentId,
        string category,
        string type,
        DateTime awardedOn)
    {
        StudentAward award = new(
            studentId,
            category,
            type,
            awardedOn);
        
        return award;
    }

    public static StudentAward Create(
        DateTime awardedOn,
        string incidentId,
        string teacherId,
        string reason,
        string studentId)
    {
        StudentAward award = new(
            studentId,
            Astra,
            Astra,
            awardedOn)
        {
            IncidentId = incidentId,
            TeacherId = teacherId,
            Reason = reason
        };
        
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

        RaiseDomainEvent(new AwardMatchedToIncidentDomainEvent(new DomainEventId(), Id));
    }

    public void CertificateDownloaded() => RaiseDomainEvent(new AwardCertificateDownloadedDomainEvent(new DomainEventId(), Id));
}
