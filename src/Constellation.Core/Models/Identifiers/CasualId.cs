namespace Constellation.Core.Models.Identifiers;

using System;

public record struct CasualId(Guid Value)
{
    public static CasualId Empty => new(Guid.Empty);

    public static CasualId FromValue(Guid value) =>
        new(value);

    public CasualId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
