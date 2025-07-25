namespace Constellation.Core.Models.Tutorials.Errors;

using Identifiers;
using Shared;
using System;

public sealed class TutorialSessionErrors
{
    public static readonly Func<TutorialSessionId, Error> NotFound = id => new(
        "Tutorial.Session.NotFound",
        $"Could not find a Session with Id {id}");

    public static Error AlreadyExists = new(
        "Tutorial.Session.AlreadyExists",
        "A session already exists for that day and time");
}