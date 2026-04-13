namespace Constellation.Core.Models.Assessments.Identifiers;

using Primitives;

public readonly record struct AssessmentDownloadId(Guid Value)
    : IStronglyTypedId<AssessmentDownloadId, Guid>
{
    public static AssessmentDownloadId Empty => new(Guid.Empty);

    public static AssessmentDownloadId FromValue(Guid value) =>
        new(value);
    
    public AssessmentDownloadId()
        : this(Guid.CreateVersion7()) { }
    
    public override string ToString() =>
        Value.ToString();
}