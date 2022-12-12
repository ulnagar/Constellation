namespace Constellation.Core.Errors;

using Constellation.Core.Shared;
using System;

public static class DomainErrors
{
    public static class GroupTutorials
    {
        public static readonly Error TutorialHasExpired = new(
            "GroupTutorials.GroupTutorial.TutorialHasExpired",
            "The Tutorial has already ended or has been deleted");

        public static readonly Error StudentAlreadyEnrolled = new(
            "GroupTutorials.GroupTutorial.StudentAlreadyEnrolled",
            "The student is already actively enrolled in this tutorial");

        public static readonly Func<DateTime, Error> RollAlreadyExistsForDate = rollDate => new Error(
            "GroupTutorials.TutorialRoll.RollAlreadyExistsForDate",
            $"A roll for date {rollDate.ToShortDateString()} already exists");
    }
}
