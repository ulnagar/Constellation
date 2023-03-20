namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TrainingModuleId(Guid Value)
{
    public TrainingModuleId()
        : this(Guid.NewGuid()) { }
}
