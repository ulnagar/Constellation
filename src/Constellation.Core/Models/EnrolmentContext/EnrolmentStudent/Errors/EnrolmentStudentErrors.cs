namespace Constellation.Core.Models.EnrolmentContext.EnrolmentStudent.Errors;

using Identifiers;
using Shared;

public static class EnrolmentStudentErrors
{
    public static readonly Func<EnrolmentStudentId, Error> NotFound = id => new(
        "Enrolment.Student.NotFound",
        $"Could not find a student with the id '{id}'");

    public static readonly Error MustHaveOneParent = new(
        "Enrolment.Student.MustHaveOneParent",
        "A student must have at least one parent linked");
}
