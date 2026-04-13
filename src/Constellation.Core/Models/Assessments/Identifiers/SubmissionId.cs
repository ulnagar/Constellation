namespace Constellation.Core.Models.Assessments.Identifiers;

using Primitives;

public readonly record struct SubmissionId(Guid Value)
    : IStronglyTypedId<SubmissionId, Guid>
{
    public static SubmissionId Empty => new(Guid.Empty);

    public static SubmissionId FromValue(Guid value) =>
        new(value);
    
    public SubmissionId()
        : this(Guid.CreateVersion7()) { }
    
    public override string ToString() =>
        Value.ToString();
}