namespace Constellation.Core.Models.MandatoryTraining.Identifiers;

using System;

public sealed record TrainingRoleId(Guid Value)
{
    public static TrainingRoleId FromValue(Guid value) =>
        new(value);

    public TrainingRoleId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}