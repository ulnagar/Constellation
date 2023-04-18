namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TrainingCompletionId(Guid Value)
{
    public static TrainingCompletionId FromValue(Guid value) =>
        new(value);

    public TrainingCompletionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
