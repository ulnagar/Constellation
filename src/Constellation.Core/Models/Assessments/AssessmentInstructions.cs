namespace Constellation.Core.Models.Assessments;

using Enums;
using Identifiers;

public sealed class AssessmentInstruction
{
    public AssessmentInstruction(
        AssessmentId assessmentId,
        UserCategory category,
        string details)
    {
        Id = new();

        AssessmentId = assessmentId;
        Category = category;
        Details = details;
    }
    
    public AssessmentInstructionId Id { get; init; }
    public AssessmentId AssessmentId { get; private set; }
    public UserCategory Category { get; private set; }
    public string Details { get; private set; }

    public void Update(string details) => Details = details;
}