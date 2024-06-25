﻿namespace Constellation.Core.Models.Training.Identifiers;

using System;

public record struct TrainingCompletionId(Guid Value)
{
    public static readonly TrainingCompletionId Empty = new(Guid.Empty);

    public static TrainingCompletionId FromValue(Guid value) =>
        new(value);

    public TrainingCompletionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
