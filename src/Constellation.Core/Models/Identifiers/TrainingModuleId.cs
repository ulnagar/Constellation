namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TrainingModuleId(Guid Value)
{
    public static TrainingModuleId FromValue(Guid value) =>
        new(value);

    public TrainingModuleId()
        : this(Guid.NewGuid()) { }
}
