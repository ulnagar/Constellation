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
    private readonly List<CourseOffering> _offerings = new();
    private readonly List<SciencePracRoll> _rolls = new();
    
    private SciencePracLesson(
        string name,
        DateOnly dueDate,
        List<CourseOffering> offerings,
        bool doNotGenerateRolls)
    {
        Id = new();

        Name = name;
        DueDate = dueDate;
        DoNotGenerateRolls = doNotGenerateRolls;

        foreach (CourseOffering offering in offerings)
            AddOffering(offering);
    }

    public SciencePracLessonId Id { get; private set; }
    public string Name { get; private set; }
    public DateOnly DueDate { get; private set; }
    public IReadOnlyList<CourseOffering> Offerings => _offerings;
    public IReadOnlyList<SciencePracRoll> Rolls => _rolls;
    public bool DoNotGenerateRolls { get; private set; }

    public static Result<SciencePracLesson> Create(
        string name,
        DateOnly dueDate,
        List<CourseOffering> offerings,
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

    public void AddOffering(CourseOffering offering)
    {
        if (_offerings.Contains(offering))
            return;

        _offerings.Add(offering);
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
}
