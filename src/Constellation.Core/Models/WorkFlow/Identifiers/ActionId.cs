namespace Constellation.Core.Models.WorkFlow.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct ActionId(Guid Value)
    : IStronglyTypedId<ActionId, Guid>
{
    public static ActionId Empty => new(Guid.Empty);

    public static ActionId FromValue(Guid value) =>
        new(value);

    public ActionId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}