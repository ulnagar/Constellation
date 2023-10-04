namespace Constellation.Core.Models.Offerings;

using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;
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

    private Offering() { }

    public Offering(
        OfferingName name, 
        CourseId courseId, 
        DateOnly startDate, 
        DateOnly endDate)
    {
        Id = new();
        Name = name;
        CourseId = courseId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public OfferingId Id { get; set; }
    public OfferingName Name { get; set; }
    public CourseId CourseId { get; set; }
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

    public void Update(
        DateOnly startDate,
        DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public Result AddTeacher(
        string staffId,
        AssignmentType type)
    {
        if (_teachers.Any(assignment => 
            assignment.StaffId == staffId && 
            assignment.Type == type && 
            !assignment.IsDeleted))
            return Result.Failure(OfferingErrors.AddTeacher.AlreadyExists);

        TeacherAssignment assignment = new TeacherAssignment(
            Id, 
            staffId, 
            type);

        _teachers.Add(assignment);

        if (_teachers.Count(assignment =>
                assignment.StaffId == staffId &&
                !assignment.IsDeleted) == 1)
            RaiseDomainEvent(new TeacherAddedToOfferingDomainEvent(new(), Id, assignment.Id));

        return Result.Success();
    }

    public Result RemoveTeacher(
        string staffId,
        AssignmentType type)
    {
        TeacherAssignment assignment = _teachers.FirstOrDefault(assignment =>
            assignment.StaffId == staffId &&
            assignment.Type == type &&
            !assignment.IsDeleted);

        if (assignment is null)
            return Result.Failure(OfferingErrors.RemoveTeacher.NotFound);

        assignment.Delete();

        if (_teachers.All(assignment =>
            assignment.StaffId == staffId &&
            assignment.IsDeleted))
            RaiseDomainEvent(new TeacherRemovedFromOfferingDomainEvent(new(), Id, assignment.Id));

        return Result.Success();
    }

    public Result AddSession(
        int periodId)
    {
        if (_sessions.Any(session => session.PeriodId == periodId && !session.IsDeleted))
            return Result.Failure(OfferingErrors.AddSession.AlreadyExists);

        Session session = new Session(
            Id,
            periodId);

        _sessions.Add(session);

        return Result.Success();
    }

    public Result RemoveSession(SessionId sessionId)
    {
        Session session = _sessions.FirstOrDefault(session => session.Id == sessionId);

        if (session is null)
            return Result.Failure(OfferingErrors.RemoveSession.NotFound);

        session.Delete();

        return Result.Success();
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

        Resource resource = type switch
        {
            var _ when type == ResourceType.AdobeConnectRoom => new AdobeConnectRoomResource(Id, resourceId, name, url),
            var _ when type == ResourceType.MicrosoftTeam => new MicrosoftTeamResource(Id, resourceId, name, url),
            var _ when type == ResourceType.CanvasCourse => new CanvasCourseResource(Id, resourceId, name, url),
            _ => null
        };

        if (resource is null)
            return Result.Failure(ResourceErrors.InvalidType(type.Value));

        _resources.Add(resource);

        RaiseDomainEvent(new ResourceAddedToOfferingDomainEvent(new(), Id, resource.Id, resource.Type));

        return Result.Success();
    }

    public Result<Resource> RemoveResource(
        ResourceId resourceId)
    {
        Resource resource = _resources.FirstOrDefault(resource =>
            resource.Id == resourceId);

        if (resource is null)
            return Result.Failure<Resource>(ResourceErrors.NotFound(resourceId));

        RaiseDomainEvent(new ResourceRemovedFromOfferingDomainEvent(new(), Id, resource));

        _resources.Remove(resource);

        return resource;
    }
}