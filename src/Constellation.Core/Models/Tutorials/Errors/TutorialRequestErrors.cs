namespace Constellation.Core.Models.Tutorials.Errors;

using Shared;

public sealed class TutorialRequestErrors
{
    public static Error AlreadyReviewed = new(
        "Tutorial.Request.AlreadyReviewed",
        "The Tutorial Request has already been reviewed");

    public static Error MustIncludeNote = new(
        "Tutorial.Request.MustIncludeNote",
        "A review of a Tutorial Request must include a note");
}