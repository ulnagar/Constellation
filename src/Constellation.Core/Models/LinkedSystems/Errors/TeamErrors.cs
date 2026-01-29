namespace Constellation.Core.Models.LinkedSystems.Errors;

using Shared;
using System;

public static class TeamErrors
{
    public static Func<Guid, Error> NotFound = id => new(
        "LinkedSystem.Teams.NotFound",
        $"Could not find a registered Microsoft Team with the id {id}");

    public static Func<string, Error> NotFoundByName = name => new(
        "LinkedSystem.Teams.NotFoundByName",
        $"Could not find a registered Microsoft Team with the name {name}");

    public static Func<string, Error> TooManyResults = name => new(
        "LinkedSystem.Teams.TooManyResults",
        $"Too many Microsoft Teams registered with the name {name}");

    public static Error NoTutorialName = new(
        "LinkedSystem.Teams.NoTutorialName",
        "Could not find a Tutorial Name in the Team Description");

    public static Func<string, Error> AlreadyExists = name => new(
        "LinkedSystem.Teams.AlreadyExists",
        $"A Microsoft Team with the name '{name}' already exists");
}