namespace Constellation.Core.Models.Tutorials;

using Abstractions.Clock;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Timetables.Identifiers;
using ValueObjects;

public sealed class Tutorial : AggregateRoot, IAuditableEntity
{
    private List<TeamsResource> _teams = [];
    private List<TutorialSession> _sessions = [];

    private Tutorial(
        TutorialName name,
        DateOnly startDate,
        DateOnly endDate)
    {
        Id = new();

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

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

    public static Result<Tutorial> Create(
        TutorialName name,
        DateOnly startDate,
        DateOnly endDate,
        IDateTimeProvider dateTime)
    {
        if (endDate < startDate)
            return Result.Failure<Tutorial>(TutorialErrors.Validation.StartDateAfterEndDate);

        if (endDate < dateTime.Today)
            return Result.Failure<Tutorial>(TutorialErrors.Validation.EndDateInPast);

        return new Tutorial(
            name,
            startDate,
            endDate);
    }

    public Result Update(
        TutorialName name,
        DateOnly startDate,
        DateOnly endDate,
        IDateTimeProvider dateTime)
    {
        if (endDate < startDate)
            return Result.Failure<Tutorial>(TutorialErrors.Validation.StartDateAfterEndDate);

        if (endDate < dateTime.Today)
            return Result.Failure<Tutorial>(TutorialErrors.Validation.EndDateInPast);

        Name = name;
        StartDate = startDate;
        EndDate = endDate;

        return Result.Success();
    }

    public void Delete() => IsDeleted = true;

    public Result AddSession(
        PeriodId periodId,
        StaffId staffId)
    {
        if (_sessions.Any(session => 
                session.PeriodId == periodId && 
                !session.IsDeleted))
            return Result.Failure(TutorialSessionErrors.AlreadyExists);

        _sessions.Add(new TutorialSession(
            periodId,
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