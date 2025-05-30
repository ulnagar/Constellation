﻿namespace Constellation.Core.Models.WorkFlow.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct CaseId(Guid Value)
    : IStronglyTypedId
{
    public static readonly CaseId Empty = new(Guid.Empty);

    public static CaseId FromValue(Guid value) =>
        new(value);

    public CaseId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}