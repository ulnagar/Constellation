namespace Constellation.Core.Models.Assessments.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AssessmentId(Guid Value)
    : IStronglyTypedId<AssessmentId, Guid>
{
    public static AssessmentId Empty => new(Guid.Empty);
    public static AssessmentId FromValue(Guid value) =>
        new(value);

    public AssessmentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}