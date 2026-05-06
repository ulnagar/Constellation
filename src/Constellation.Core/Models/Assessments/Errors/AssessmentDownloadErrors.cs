namespace Constellation.Core.Models.Assessments.Errors;

using Identifiers;
using Shared;

public static class AssessmentDownloadErrors
{
    public static readonly Func<AssessmentDownloadId, Error> NotFound = id => new(
        "Assessment.Download.NotFound",
        $"An Assessment Download with the Id '{id}' could not be found");
}