namespace Constellation.Core.Models.Offerings;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Offering : AggregateRoot
{
    private readonly List<Resource> _resources = new();
    private readonly List<Session> _sessions = new();
    private readonly List<TeacherAssignment> _teachers = new();

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
    public IReadOnlyList<Session> Sessions => _sessions;
    public IReadOnlyList<Resource> Resources => _resources;
    public IReadOnlyList<TeacherAssignment> Teachers => _teachers;
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

    public void AddTeacher(
        string staffId,
        AssignmentType type)
    {
        if (_teachers.Any(assignment => 
            assignment.StaffId == staffId && 
            assignment.Type == type && 
            !assignment.IsDeleted))
            return;

        TeacherAssignment assignment = new TeacherAssignment(
            Id, 
            staffId, 
            type);

        _teachers.Add(assignment);

        if (_teachers.Count(assignment =>
            assignment.StaffId == staffId &&
            !assignment.IsDeleted) > 1)
            return;

        RaiseDomainEvent(new TeacherAddedToOfferingDomainEvent(new(), Id, assignment.Id));
    }

    public void RemoveTeacher(
        string staffId,
        AssignmentType type)
    {
        TeacherAssignment assignment = _teachers.FirstOrDefault(assignment =>
            assignment.StaffId == staffId &&
            assignment.Type == type &&
            !assignment.IsDeleted);

        if (assignment is null)
            return;

        assignment.Delete();

        if (_teachers.Any(assignment =>
            assignment.StaffId == staffId &&
            !assignment.IsDeleted))
            return;

        RaiseDomainEvent(new TeacherRemovedFromOfferingDomainEvent(new(), Id, assignment.Id));
    }

    public void AddSession(
        int periodId)
    {
        if (_sessions.Any(session => session.PeriodId == periodId && !session.IsDeleted))
            return;

        Session session = new Session(
            Id,
            periodId);

        _sessions.Add(session);
    }

    public void RemoveSession(SessionId sessionId)
    {
        Session session = _sessions.FirstOrDefault(session => session.Id == sessionId);

        if (session is null)
            return;

        session.Delete();
    }

    public void RemoveAllSessions()
    {
        foreach (Session session in _sessions)
        {
            if (session.IsDeleted) continue;

            session.Delete();
        }
    }

    public Result AddResource(
        ResourceType type,
        string resourceId,
        string name,
        string url)
    {
        if (_resources.Any(resource => resource.Type == type && resource.ResourceId == resourceId))
            return Result.Success();

        Resource resource = type.Value switch
        {
            "Adobe Connect Room" => new AdobeConnectRoomResource(Id, url, name, resourceId),
            "Microsoft Team" => new MicrosoftTeamResource(Id, url, name, resourceId),
            "Canvas Course" => new CanvasCourseResource(Id, url, name, resourceId),
            _ => null
        };

        if (resource is null)
            return Result.Failure(ResourceErrors.InvalidType(type.Value));

        _resources.Add(resource);

        RaiseDomainEvent(new ResourceAddedToOfferingDomainEvent(new(), Id, resource.Id, resource.Type));

        return Result.Success();
    }
}