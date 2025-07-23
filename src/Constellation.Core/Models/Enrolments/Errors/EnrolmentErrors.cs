namespace Constellation.Core.Models.Enrolments.Errors;

using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Tutorials.Identifiers;
using Identifiers;
using Shared;
using System;

public static class EnrolmentErrors
{
    public static readonly Func<EnrolmentId, Error> NotFound = id => new(
        "Enrolment.NotFound",
        $"Could not find an Enrolment with the Id {id}");
    public static Error AlreadyDeleted => new(
        "Enrolment.AlreadyDeleted",
        "This enrolment is already marked deleted");

    public static readonly Func<StudentId, TutorialId, Error> AlreadyExistsForTutorial = (studentId, tutorialId) => new(
        "Enrolments.AlreadyExists",
        $"A current enrolment already exists for student {studentId} and tutorial {tutorialId}");
}
