﻿namespace Constellation.Core.Models.Training.Identifiers;

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