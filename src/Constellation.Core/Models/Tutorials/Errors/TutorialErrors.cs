namespace Constellation.Core.Models.Tutorials.Errors;

using Identifiers;
using Shared;
using System;

public sealed class TutorialErrors
{
    public static readonly Func<TutorialId, Error> NotFound = id => new(
        "Tutorial.NotFound",
        $"Could not find Tutorial with Id {id}");

    public static Error TeamAlreadyExists = new(
        "Tutorial.Team.AlreadyExists",
        "The Team is already linked to the Tutorial");

    public static class Validation
    {

        public static Error AlreadyExists = new(
            "Tutorial.Validation.AlreadyExists",
            "A tutorial with conflicting details already exists");

        public static readonly Error StartDateAfterEndDate = new(
            "Tutorial.Validation.StartDate",
            "Start Date cannot be after the End Date");

        public static readonly Error EndDateInPast = new(
            "Tutorial.Validation.EndDate",
            "End Date cannot be in the past");
    }
}