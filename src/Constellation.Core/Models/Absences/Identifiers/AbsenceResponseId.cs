namespace Constellation.Core.Models.Absences.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AbsenceResponseId(Guid Value)
    : IStronglyTypedId
{
    public static AbsenceResponseId Empty = new(Guid.Empty);

    public static AbsenceResponseId FromValue(Guid Value) =>
        new(Value);

    public AbsenceResponseId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}
