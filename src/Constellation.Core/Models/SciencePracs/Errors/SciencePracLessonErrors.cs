namespace Constellation.Core.Models.SciencePracs.Errors;

using Constellation.Core.Models.Identifiers;
using Shared;
using System;

public static class SciencePracLessonErrors
{
    public static readonly Func<DateOnly, Error> PastDueDate = date => new(
        "SciencePracs.Lesson.PastDueDate",
        $"Cannot create a Lesson due in the past ({date})");

    public static readonly Error EmptyName = new(
        "SciencePracs.Lesson.EmptyName",
        "Cannot create a Lesson with a blank name");

    public static readonly Func<SciencePracLessonId, Error> NotFound = id => new(
        "SciencePracs.Lesson.NotFound",
        $"Could not find a lesson with the Id {id}");

    public static readonly Error RollCompleted = new(
        "SciencePracs.Lesson.RollCompleted",
        "Cannot cancel a lesson that has completed rolls");

    public static readonly Error CannotEdit = new(
        "SciencePracs.Lesson.CannotEdit",
        "Cannot edit a lesson after a roll has been submitted");

    public static readonly Error NoOfferingsLinked = new(
        "SciencePracs.Lesson.NoOfferingsLinked",
        "There are no Offerings linked to the selected Science Prac Lesson");
}