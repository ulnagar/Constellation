namespace Constellation.Core.Models.Rollover.Errors;

using Shared;
using System;

public static class RolloverErrors
{
    public static readonly Error StudentIdEmpty = new(
        "Rollover.AddDecision.StudentIdEmpty",
        "Cannot register a Decision without a student Id");

    public static readonly Func<string, Error> AlreadyExists = id => new(
        "Rollover.AddDecision.AlreadyExists",
        $"A Decision has already been registered for student with Id {id}");

    public static readonly Error RolloverImpossible = new(
        "Rollover.Process.RolloverImpossible",
        "Cannot roll over the student due to no Grade available to roll into");

    public static readonly Error InvalidDecision = new(
        "Rollover.Process.InvalidDecision",
        "Cannot process an invalid decision");
}
