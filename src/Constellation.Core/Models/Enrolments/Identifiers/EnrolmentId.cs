namespace Constellation.Core.Models.Enrolments.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record EnrolmentId(Guid Value)
    : IStronglyTypedId
{
    public static EnrolmentId FromValue(Guid Value) =>
        new(Value);

    public EnrolmentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}