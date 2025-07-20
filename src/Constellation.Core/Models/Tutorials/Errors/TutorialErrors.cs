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
}