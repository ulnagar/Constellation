namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;
using System.Text.RegularExpressions;

public readonly record struct CasualId(Guid Value)
    : IStronglyTypedId<CasualId, Guid>
{
    public static CasualId Empty => new(Guid.Empty);

    public static CasualId FromValue(Guid value) =>
        new(value);

    public CasualId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}