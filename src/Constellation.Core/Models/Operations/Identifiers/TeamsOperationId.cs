namespace Constellation.Core.Models.Operations.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct TeamsOperationId(Guid Value)
    : IStronglyTypedId
{
    public static readonly TeamsOperationId Empty = new(Guid.Empty);

    public static TeamsOperationId FromValue(Guid value) =>
        new(value);

    public TeamsOperationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}