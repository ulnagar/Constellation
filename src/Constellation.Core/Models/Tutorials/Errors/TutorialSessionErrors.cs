namespace Constellation.Core.Models.Tutorials.Errors;

using Shared;

public sealed class TutorialSessionErrors
{
    public static Error AlreadyExists = new(
        "Tutorial.Session.AlreadyExists",
        "A session already exists for that day and time");
}