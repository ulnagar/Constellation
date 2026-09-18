namespace Constellation.Core.Models.GroupTutorials.Errors;

using Constellation.Core.Models.Identifiers;
using Shared;
using System;

public static class GroupTutorialErrors
{
    public static readonly Error TutorialHasExpired = new(
        "GroupTutorials.GroupTutorial.TutorialHasExpired",
        "The Tutorial has already ended or has been deleted");

    public static readonly Func<GroupTutorialId, Error> NotFound = id => new Error(
        "GroupTutorials.GroupTutorial.NotFound",
        $"A tutorial with the Id {id.Value} could not be found");

    public static readonly Error CouldNotCreateTutorial = new(
        "GroupTutorials.GroupTutorial.CouldNotCreate",
        "There was an error attempting to create the group tutorial");
}