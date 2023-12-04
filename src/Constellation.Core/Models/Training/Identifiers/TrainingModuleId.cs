namespace Constellation.Core.Models.Training.Identifiers;

using System;

public sealed record TrainingModuleId(Guid Value)
{
    public static TrainingModuleId FromValue(Guid value) =>
        new(value);

    public TrainingModuleId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}