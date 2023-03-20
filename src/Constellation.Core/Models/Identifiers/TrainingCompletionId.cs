namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record TrainingCompletionId(Guid Value)
{
    public TrainingCompletionId()
        : this(Guid.NewGuid()) { }
}
