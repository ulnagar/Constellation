namespace Constellation.Core.Models.Absences.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record AbsenceId(Guid Value)
    : IStronglyTypedId
{
    public static AbsenceId FromValue(Guid value) =>
        new(value);

    public AbsenceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
