namespace Constellation.Core.Models.Training.Identifiers;

using System;

public record struct TrainingCompletionId(Guid Value)
{
    public static TrainingCompletionId FromValue(Guid value) =>
        new(value);

    public TrainingCompletionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
