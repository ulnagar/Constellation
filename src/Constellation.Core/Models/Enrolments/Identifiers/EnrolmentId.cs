﻿namespace Constellation.Core.Models.Enrolments.Identifiers;

using System;

public sealed record EnrolmentId(Guid Value)
{
    public static EnrolmentId FromValue(Guid Value) =>
        new(Value);

    public EnrolmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}