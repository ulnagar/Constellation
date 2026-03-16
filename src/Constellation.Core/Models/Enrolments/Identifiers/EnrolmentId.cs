namespace Constellation.Core.Models.Enrolments.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct EnrolmentId(Guid Value)
    : IStronglyTypedId
{
    public static EnrolmentId FromValue(Guid Value) =>
        new(Value);

    public EnrolmentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}