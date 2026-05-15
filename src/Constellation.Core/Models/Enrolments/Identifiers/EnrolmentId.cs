namespace Constellation.Core.Models.Enrolments.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct EnrolmentId(Guid Value)
    : IStronglyTypedId<EnrolmentId, Guid>
{
    public static EnrolmentId Empty => new(Guid.Empty);

    public static EnrolmentId FromValue(Guid value) =>
        new(value);

    public EnrolmentId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}