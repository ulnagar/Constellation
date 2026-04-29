namespace Constellation.Core.Models.Assessments.Identifiers;

using Primitives;

public readonly record struct AssessmentInstructionId(Guid Value)
    : IStronglyTypedId<AssessmentInstructionId, Guid>
{
    public static AssessmentInstructionId Empty => new(Guid.Empty);

    public static AssessmentInstructionId FromValue(Guid value) =>
        new(value);

    public AssessmentInstructionId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}