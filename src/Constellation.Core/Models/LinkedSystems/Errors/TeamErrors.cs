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

    public static Func<string, Error> AlreadyExistsByName = name => new(
        "LinkedSystem.Teams.AlreadyExists",
        $"A Microsoft Team with the name '{name}' already exists");

    public static readonly Error TeamNotFoundInDatabase = new(
        "LinkedSystems.Teams.TeamNotFoundInDatabase",
        "The Team could not be found in the database");

    public static readonly Error MoreThanOneMatchFound = new(
        "LinkedSystems.Teams.MoreThanOneMatchFound",
        "Found more than one Team that matched the criteria in the database");

    public static readonly Func<Guid, Error> AlreadyExists = id => new Error(
        "LinkedSystems.Teams.AlreadyExists",
        $"The Team with Id {id} could not be created because it already exists in the database");

}