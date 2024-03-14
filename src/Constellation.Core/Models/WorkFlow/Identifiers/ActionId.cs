namespace Constellation.Core.Models.WorkFlow.Identifiers;

using System;

public sealed record ActionId(Guid Value)
{
    public static ActionId FromValue(Guid value) =>
        new(value);

    public ActionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}