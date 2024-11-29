namespace Constellation.Core.Models.Timetables.Identifiers;

using Primitives;
using System;

public readonly record struct PeriodId(Guid Value)
    :IStronglyTypedId
{
    public static readonly PeriodId Empty = new(Guid.Empty);

    public static PeriodId FromValue(Guid value) =>
        new(value);

    public PeriodId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}