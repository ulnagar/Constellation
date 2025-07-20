namespace Constellation.Core.Models.Tutorials.Identifiers;

using Primitives;
using System;

public readonly record struct TeamsResourceId(Guid Value)
    : IStronglyTypedId
{
    public static readonly TeamsResourceId Empty = new(Guid.Empty);

    public static TeamsResourceId FromValue(Guid value) =>
        new(value);

    public TeamsResourceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}