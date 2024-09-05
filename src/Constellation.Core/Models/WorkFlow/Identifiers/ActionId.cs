namespace Constellation.Core.Models.WorkFlow.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record ActionId(Guid Value)
    : IStronglyTypedId
{
    public static readonly ActionId Empty = new(Guid.Empty);

    public static ActionId FromValue(Guid value) =>
        new(value);

    public ActionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}