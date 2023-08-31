namespace Constellation.Core.Models.Offerings;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Offering : AggregateRoot
{
    private readonly List<Resource> _resources = new();
    private readonly List<Session> _sessions = new();

    public Offering()
    {
    }

    public Offering(int courseId, DateOnly startDate, DateOnly endDate)
    {
        CourseId = courseId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public OfferingId Id { get; set; }
    public string Name { get; set; }
    public int CourseId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<Enrolment> Enrolments { get; set; } = new();
    public IReadOnlyList<Session> Sessions => _sessions;
    public IReadOnlyList<Resource> Resources => _resources;
    public List<Absence> Absences { get; set; } = new();
    public bool IsCurrent => IsOfferingCurrent();

    private bool IsOfferingCurrent()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        if (Sessions.All(s => s.IsDeleted))
            return false;

        if (StartDate <= today && EndDate >= today)
            return true;

        return false;
    }

    public void AddSession(
        string staffId,
        int periodId,
        string roomId)
    {
        // TODO: Add validation and Domain Event

        Session session = new Session(
            Id,
            staffId,
            periodId,
            roomId);

        _sessions.Add(session);
    }

    public void DeleteSession(int sessionId)
    {
        Session session = _sessions.FirstOrDefault(session => session.Id == sessionId);

        if (session is null)
            return;

        session.Delete();

        RaiseDomainEvent(new SessionDeletedDomainEvent(new DomainEventId(), Id, sessionId));
    }

    public void DeleteAllSessions()
    {
        List<string> staffIds = new();
        List<string> roomIds = new();

        foreach (Session session in _sessions)
        {
            if (session.IsDeleted) continue;

            staffIds.Add(session.StaffId);
            roomIds.Add(session.RoomId);

            session.Delete();
        }

        staffIds = staffIds.Distinct().ToList();
        roomIds = roomIds.Distinct().ToList();

        RaiseDomainEvent(new AllSessionsDeletedDomainEvent(new DomainEventId(), Id, staffIds, roomIds));
    }
}