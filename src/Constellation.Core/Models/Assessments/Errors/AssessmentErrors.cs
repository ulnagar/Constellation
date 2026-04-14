namespace Constellation.Core.Models.Assessments.Errors;

using Identifiers;
using Shared;
using Students.Identifiers;

public static class AssessmentErrors
{
    public static Func<AssessmentId, Error> NotFound = id => new(
        "Assessment.NotFound",
        $"An Assessment with the Id '{id}' could not be found");

    public static Func<StudentId, Error> NoLinkedStudent = id => new(
        "Assessment.NoLinkedStudent",
        $"No student with the Id '{id}' could be found in the Assessment");

    public static Func<StudentId, Error> StudentAlreadyExists = id => new(
        "Assessment.StudentAlreadyExists",
        $"A student with the Id '{id}' already exists in the Assessment");
}