namespace Constellation.Core.Models.GroupTutorials.Errors;

using Identifiers;
using Shared;
using Students.Identifiers;

public static class GroupTutorialRollErrors
{
    public static readonly Func<DateOnly, Error> RollAlreadyExistsForDate = rollDate => new Error(
        "GroupTutorials.TutorialRoll.RollAlreadyExistsForDate",
        $"A roll for date {rollDate.ToShortDateString()} already exists");

    public static readonly Func<DateOnly, Error> RollDateInvalid = rollDate => new Error(
        "GroupTutorials.TutorialRoll.RollDateInvalid",
        $"Cannot create a roll for {rollDate.ToShortDateString()} as this is not a valid date for this tutorial");

    public static readonly Func<TutorialRollId, Error> NotFound = id => new Error(
        "GroupTutorials.TutorialRoll.NotFound",
        $"A roll with the Id {id.Value} could not be found");

    public static readonly Error SubmitInvalidStatus = new(
        "GroupTutorials.TutorialRoll.SubmitInvalidStatus",
        "Cannot submit a roll that has been cancelled or previously submitted");

    public static readonly Func<StudentId, Error> StudentNotFound = student => new Error(
        "GroupTutorials.TutorialRoll.StudentNotFound",
        $"Cannot find an attendance record for student with Id {student} attached to the roll");

    public static readonly Func<StudentId, Error> RemoveEnrolledStudent = student => new Error(
        "GroupTutorials.TutorialRoll.RemoveEnrolledStudent",
        $"Cannot remove student with Id {student} from the roll as they are enrolled in the tutorial");
}