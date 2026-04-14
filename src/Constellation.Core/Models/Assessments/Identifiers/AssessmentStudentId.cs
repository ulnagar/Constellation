namespace Constellation.Core.Models.Assessments.Identifiers;

using Primitives;

public readonly record struct AssessmentStudentId(Guid Value)
    : IStronglyTypedId<AssessmentStudentId, Guid>
{
    public static AssessmentStudentId Empty => new(Guid.Empty);
    public static AssessmentStudentId FromValue(Guid value) =>
        new(value);
    public AssessmentStudentId()
        : this(Guid.CreateVersion7()) { }
    public override string ToString() =>
        Value.ToString();
}