namespace Constellation.Core.Models.Training.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct TrainingModuleId(Guid Value)
    : IStronglyTypedId
{
    public static readonly TrainingModuleId Empty = new(Guid.Empty);

    public static TrainingModuleId FromValue(Guid value) =>
        new(value);

    public TrainingModuleId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}