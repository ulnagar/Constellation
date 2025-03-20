using Constellation.Core.Models.Students.Identifiers;

namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Models.Offerings.Identifiers;
using DomainEvents;
using Enums;
using Errors;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class SciencePracLesson : AggregateRoot
{
    private readonly List<SciencePracLessonOffering> _offerings = new();
    private readonly List<SciencePracRoll> _rolls = new();
    
    // Required for EF Core
    private SciencePracLesson() { }

    private SciencePracLesson(
        string name,
        DateOnly dueDate,
        Grade grade,
        List<OfferingId> offerings,
        bool doNotGenerateRolls)
    {
        Id = new();

        Name = name;
        DueDate = dueDate;
        DoNotGenerateRolls = doNotGenerateRolls;

        foreach (OfferingId offering in offerings)
            AddOffering(offering);

        RaiseDomainEvent(new SciencePracLessonCreatedDomainEvent(new(), Id));
    }

    public SciencePracLessonId Id { get; private set; }
    public string Name { get; private set; }
    public DateOnly DueDate { get; private set; }
    public Grade Grade { get; private set; }
    public IReadOnlyList<SciencePracLessonOffering> Offerings => _offerings;
    public IReadOnlyList<SciencePracRoll> Rolls => _rolls;
    public bool DoNotGenerateRolls { get; private set; }

    public static Result<SciencePracLesson> Create(
        string name,
        DateOnly dueDate,
        Grade grade,
        List<OfferingId> offerings,
        bool doNotGenerateRolls)
    {
        if (dueDate < DateOnly.FromDateTime(DateTime.Today))
        {
            return Result.Failure<SciencePracLesson>(SciencePracLessonErrors.PastDueDate(dueDate));
        }

        return new SciencePracLesson(
            name,
            dueDate,
            grade,
            offerings,
            doNotGenerateRolls);
    }

    public Result MarkRoll(
        SciencePracRollId rollId,
        string submittedBy,
        DateOnly lessonDate,
        string comment,
        List<StudentId> presentStudents,
        List<StudentId> absentStudents)
    {
        SciencePracRoll roll = _rolls.FirstOrDefault(roll => roll.Id == rollId);

        if (roll is null)
        {
            return Result.Failure(SciencePracRollErrors.NotFound(rollId));
        }

        Result attempt = roll.MarkRoll(
            submittedBy,
            lessonDate,
            comment,
            presentStudents,
            absentStudents);

        if (attempt.IsFailure)
            return attempt;

        RaiseDomainEvent(new SciencePracRollSubmittedDomainEvent(new(), Id, rollId));

        return Result.Success();
    }

    public void AddOffering(OfferingId offering)
    {
        if (_offerings.Any(entry => entry.OfferingId == offering))
            return;

        _offerings.Add(new (Id, offering));
    }

    public Result AddRoll(SciencePracRoll roll)
    {
        if (_rolls.Any(entry => entry.SchoolCode == roll.SchoolCode))
            return Result.Failure(SciencePracRollErrors.AlreadyExistsForSchool);

        _rolls.Add(roll);

        return Result.Success();
    }

    public void UpdateGrade(Grade grade) => Grade = grade;

    public Result Update(
        string name,
        DateOnly dueDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(SciencePracLessonErrors.EmptyName);

        if (dueDate < DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure(SciencePracLessonErrors.PastDueDate(dueDate));

        Name = name;
        DueDate = dueDate;

        return Result.Success();
    }

    public void Cancel()
    {
        foreach (SciencePracRoll roll in _rolls)
            roll.CancelRoll("Lesson Cancelled");

        DoNotGenerateRolls = true;
    }
}
