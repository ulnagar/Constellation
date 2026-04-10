namespace Constellation.Core.Models.Training.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct TrainingCompletionId(Guid Value)
    : IStronglyTypedId<TrainingCompletionId, Guid>
{
    public static readonly TrainingCompletionId Empty = new(Guid.Empty);

    public static TrainingCompletionId FromValue(Guid value) =>
        new(value);

    public TrainingCompletionId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}
