namespace Constellation.Core.Models.Enrolments.Errors;

using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Tutorials.Identifiers;
using Identifiers;
using Offerings.Identifiers;
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

    public static readonly Func<StudentId, Error> NotFoundForStudent = id => new Error(
        "Enrolments.Enrolment.NotFoundForStudent",
        $"No enrolments could be found for student with Id {id}");

    public static readonly Func<StudentId, OfferingId, Error> AlreadyExists = (studentId, offeringId) => new(
        "Enrolments.Enrolment.AlreadyExists",
        $"A current enrolment already exists for student {studentId} and offering {offeringId}");

}
