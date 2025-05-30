﻿namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct ParentId(Guid Value)
    : IStronglyTypedId
{
    public static ParentId Empty => new(Guid.Empty);

    public static ParentId FromValue(Guid value) =>
        new(value);

    public ParentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
