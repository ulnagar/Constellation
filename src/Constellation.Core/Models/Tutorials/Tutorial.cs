namespace Constellation.Core.Models.Tutorials;

using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Timetables.Enums;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class Tutorial : AggregateRoot, IAuditableEntity
{
    private List<TeamsResource> _teams = [];
    private List<TutorialSession> _sessions = [];

    public TutorialId Id { get; private set; }
    public TutorialName Name { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    public IReadOnlyList<TeamsResource> Teams => _teams.AsReadOnly();
    public IReadOnlyList<TutorialSession> Sessions => _sessions.AsReadOnly();

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public bool IsCurrent => IsTutorialCurrent();

    private bool IsTutorialCurrent()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        if (_sessions.Count == 0 || _sessions.All(session => session.IsDeleted))
            return false;
        
        if (StartDate <= today && EndDate >= today)
            return true;

        return false;
    }

    public void Delete() => IsDeleted = true;

    public Result AddSession(
        PeriodWeek week,
        PeriodDay day,
        TimeSpan startTime,
        TimeSpan endTime,
        StaffId staffId)
    {
        if (_sessions.Any(session =>
                session.Week.Equals(week) &&
                session.Day.Equals(day) &&
                session.StartTime <= endTime &&
                session.EndTime >= startTime))
            return Result.Failure(TutorialSessionErrors.AlreadyExists);

        _sessions.Add(new TutorialSession(
            week,
            day,
            startTime,
            endTime,
            staffId));

        return Result.Success();
    }

    public void DeleteSession(TutorialSession session) => session.Delete();

    public Result AddTeam(
        Guid teamId,
        string name,
        string url)
    {
        if (_teams.Any(team => team.TeamId == teamId))
            return Result.Failure(TutorialErrors.TeamAlreadyExists);

        _teams.Add(new(
            teamId,
            name,
            url));

        return Result.Success();
    }

    public void DeleteTeam(TeamsResource resource) => _teams.Remove(resource);
}