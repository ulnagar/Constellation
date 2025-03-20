namespace Constellation.Core.Models.SciencePracs.Errors;

using Identifiers;
using Shared;
using System;

public static class SciencePracRollErrors
{
    public static readonly Error AlreadyExistsForSchool = new(
        "SciencePracs.Roll.AlreadyExistsForSchool",
        "A Roll already exists in this Lesson for this school");

    public static readonly Error CommentRequiredNonePresent = new(
        "SciencePracs.Rolls.CommentRequiredNonePresent",
        "Cannot mark a roll with no students present without providing a comment");

    public static readonly Error CannotCancelCompletedRoll = new(
        "SciencePracs.Rolls.CannotCancelCompletedRolls",
        "Cannot mark roll cancelled if it has already been submitted as marked");

    public static readonly Error CannotReinstateRoll = new(
        "SciencePracs.Rolls.CannotReinstateRoll",
        "Cannot reinstate a roll that has not been cancelled");

    public static readonly Func<SciencePracRollId, Error> NotFound = id => new(
        "SciencePracs.Rolls.NotFound",
        $"Could not find a roll with the Id {id}");

    public static readonly Error MustBeCancelled = new(
        "SciencePracs.Rolls.MustBeCancelled",
        "Cannot reinstate roll that has not been cancelled");
}