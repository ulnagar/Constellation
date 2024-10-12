namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record AbsenceId(Guid Value)
    : IStronglyTypedId
{
    public static AbsenceId FromValue(Guid Value) =>
        new(Value);

    public AbsenceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
