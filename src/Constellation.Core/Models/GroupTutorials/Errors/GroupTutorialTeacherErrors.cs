namespace Constellation.Core.Models.GroupTutorials.Errors;

using Shared;

public static class GroupTutorialTeacherErrors
{
    public static readonly Error NotFound = new(
        "GroupTutorials.TutorialTeacher.EntryNotFound",
        "There is no corresponding teacher record for that tutorial");

    public static readonly Error TimeExpired = new(
        "GroupTutorials.TutorialTeacher.TimeExpired",
        "The limited access window for this teacher has expired");
}