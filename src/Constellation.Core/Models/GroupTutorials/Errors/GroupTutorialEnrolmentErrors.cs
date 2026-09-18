namespace Constellation.Core.Models.GroupTutorials.Errors;

using Shared;

public static class GroupTutorialEnrolmentErrors
{
    public static readonly Error NotFound = new(
        "GroupTutorials.TutorialEnrolment.EntryNotFound",
        "There is no corresponding enrolment record for that tutorial");

    public static readonly Error StudentAlreadyEnrolled = new(
        "GroupTutorials.TutorialEnrolment.StudentAlreadyEnrolled",
        "The student is already actively enrolled in this tutorial");

    public static readonly Error TimeExpired = new(
        "GroupTutorials.TutorialEnrolment.TimeExpired",
        "The limited access window for this student has expired");
}