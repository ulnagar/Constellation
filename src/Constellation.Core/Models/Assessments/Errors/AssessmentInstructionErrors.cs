namespace Constellation.Core.Models.Assessments.Errors;

using Shared;

public static class AssessmentInstructionErrors
{
    public static readonly Error InvalidId = new(
        "Assessment.Instruction.InvalidId",
        "The supplied Id is invalid");
}