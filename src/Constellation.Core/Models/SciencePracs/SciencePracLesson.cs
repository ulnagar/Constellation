namespace Constellation.Core.Models.SciencePracs;

using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class SciencePracLesson : AggregateRoot
{
    private readonly List<SciencePracLessonOffering> _offerings = new();
    private readonly List<SciencePracRoll> _rolls = new();
    
    private SciencePracLesson(
        string name,
        DateOnly dueDate,
        List<int> offerings,
        bool doNotGenerateRolls)
    {
        Id = new();

        Name = name;
        DueDate = dueDate;
        DoNotGenerateRolls = doNotGenerateRolls;

        foreach (int offering in offerings)
            AddOffering(offering);
    }

    public SciencePracLessonId Id { get; private set; }
    public string Name { get; private set; }
    public DateOnly DueDate { get; private set; }
    public IReadOnlyList<SciencePracLessonOffering> Offerings => _offerings;
    public IReadOnlyList<SciencePracRoll> Rolls => _rolls;
    public bool DoNotGenerateRolls { get; private set; }

    public static Result<SciencePracLesson> Create(
        string name,
        DateOnly dueDate,
        List<int> offerings,
        bool doNotGenerateRolls)
    {
        if (dueDate < DateOnly.FromDateTime(DateTime.Today))
        {
            return Result.Failure<SciencePracLesson>(DomainErrors.SciencePracs.Lesson.PastDueDate(dueDate));
        }

        return new SciencePracLesson(
            name,
            dueDate,
            offerings,
            doNotGenerateRolls);
    }

    public void AddOffering(int offering)
    {
        if (_offerings.Any(entry => entry.OfferingId == offering))
            return;

        _offerings.Add(new (Id, offering));
    }

    public Result AddRoll(SciencePracRoll roll)
    {
        if (_rolls.Any(entry => entry.SchoolCode == roll.SchoolCode))
            return Result.Failure(DomainErrors.SciencePracs.Roll.AlreadyExistsForSchool);

        _rolls.Add(roll);

        return Result.Success();
    }

    public Result Update(
        string name,
        DateOnly dueDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(DomainErrors.SciencePracs.Lesson.EmptyName);

        if (dueDate < DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure(DomainErrors.SciencePracs.Lesson.PastDueDate(dueDate));

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
