namespace Constellation.Core.Models.Assessments.Errors;

using Identifiers;
using Shared;

public static class AssessmentSubmissionErrors
{
    public static Func<SubmissionId, Error> NotFound = id => new(
        "Assessment.Submission.NotFound",
        $"An Assessment Submission with the Id '{id}' could not be found");
}