namespace Constellation.Core.Models.Awards;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using System;

public class StudentAward : AggregateRoot
{
    private StudentAward(
        StudentAwardId id,
        string studentId,
        string category,
        string type,
        DateOnly awardedOn)
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
    public DateOnly AwardedOn { get; private set; }
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
            DateOnly.FromDateTime(awardedOn));

        return award;
    }

    public static StudentAward Create(
        StudentAwardId id,
        DateOnly awardedOn,
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

        return award;
    }
}
