namespace Constellation.Core.Models.Absences.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AbsenceId(Guid Value)
    : IStronglyTypedId<AbsenceId, Guid>
{
    public static AbsenceId FromValue(Guid value) =>
        new(value);

    public AbsenceId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}
